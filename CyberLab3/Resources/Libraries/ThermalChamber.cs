using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

public enum Mb1OperatingMode
{
    Unknown = 0,    // Reprezentuje nieznany lub nieobsługiwany tryb
    Basic = 0x1000, // Bit 12
    Manual = 0x0800,  // Bit 11
    Auto = 0x0400     // Bit 10
}

public class ThermalChamber : IDisposable
{
    private const ushort ADDR_TEMP_PROCESS_VALUE = 0x11A9;
    private const ushort ADDR_HUMID_PROCESS_VALUE = 0x11CD;
    private const ushort ADDR_TEMP_SETPOINT_BASIC = 0x156F;
    private const ushort ADDR_HUMID_SETPOINT_BASIC = 0x1571;
    private const ushort ADDR_MODE_READ_WRITE = 0x1A22;

    private readonly string _host;
    private readonly int _port;
    private readonly byte _slaveId;

    private TcpClient _client;
    private NetworkStream _stream;

    public ThermalChamber(string host, int port = 10001, byte slaveId = 1)
    {
        _host = host;
        _port = port;
        _slaveId = slaveId;
        _client = new TcpClient();
    }

    public void Connect()
    {
        if (_client.Connected) return;
        try
        {
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            _stream.ReadTimeout = 2000; // 2 sekundy na odpowiedź
        }
        catch (Exception ex)
        {
            throw new Exception($"Nie można połączyć się z {_host}:{_port}. Błąd: {ex.Message}", ex);
        }
    }
    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
    }
    public float ReadTemperature()
    {
        return ReadHoldingRegistersFloat(ADDR_TEMP_PROCESS_VALUE);
    }
    public float ReadHumidity()
    {
        return ReadHoldingRegistersFloat(ADDR_HUMID_PROCESS_VALUE);
    }
    public Mb1OperatingMode ReadOperatingMode()
    {
        ushort modeValue = ReadSingleRegister(ADDR_MODE_READ_WRITE);

        // Sprawdzenie, który bit jest ustawiony (zgodnie z dokumentacją)
        if ((modeValue & (ushort)Mb1OperatingMode.Basic) != 0)
            return Mb1OperatingMode.Basic;
        if ((modeValue & (ushort)Mb1OperatingMode.Manual) != 0)
            return Mb1OperatingMode.Manual;
        if ((modeValue & (ushort)Mb1OperatingMode.Auto) != 0)
            return Mb1OperatingMode.Auto;

        return Mb1OperatingMode.Unknown;
    }

    public void SetOperatingMode(Mb1OperatingMode mode)
    {
        if (mode == Mb1OperatingMode.Unknown)
            throw new ArgumentException("Nie można ustawić nieznanego trybu pracy.");

        // Zgodnie z uwagą na stronie 24, należy najpierw wyzerować bity trybu, a potem ustawić nowy.
        // W praktyce, proste nadpisanie wartością z enuma powinno wystarczyć.
        WriteSingleRegister(ADDR_MODE_READ_WRITE, (ushort)mode);
    }
    public void SetTemperature(float temperature)
    {
        WriteMultipleRegistersFloat(ADDR_TEMP_SETPOINT_BASIC, temperature);
    }
    public void SetHumidity(float humidity)
    {
        WriteMultipleRegistersFloat(ADDR_HUMID_SETPOINT_BASIC, humidity);
    }
    private ushort ReadSingleRegister(ushort address)
    {
        // Użyjemy funkcji 0x03 (Read Holding Registers) do odczytu jednego rejestru
        byte[] request = BuildRequestFrame(_slaveId, 0x03, address, 1);
        byte[] response = ExecuteRequest(request);

        // Odpowiedź: [ID][Func][ByteCount=2][Val Hi][Val Lo][CRC] - łącznie 7 bajtów
        if (response.Length != 7 || response[0] != _slaveId || response[1] != 0x03 || response[2] != 2)
        {
            throw new Exception("Otrzymano nieprawidłową odpowiedź od urządzenia przy odczycie rejestru.");
        }

        // Złączenie bajtów Hi i Lo w ushort (16-bit)
        return (ushort)((response[3] << 8) | response[4]);
    }
    private float ReadHoldingRegistersFloat(ushort startAddress)
    {
        // Funkcja 0x03 - Read Holding Registers
        // Liczba zmiennoprzecinkowa (float) zajmuje 2 rejestry (4 bajty)
        byte[] request = BuildRequestFrame(_slaveId, 0x03, startAddress, 2);
        byte[] response = ExecuteRequest(request);

        // Sprawdzenie poprawności odpowiedzi (ID, kod funkcji, długość)
        // Odpowiedź: [ID][Func][ByteCount][Data...][CRC]
        if (response.Length != 9 || response[0] != _slaveId || response[1] != 0x03 || response[2] != 4)
        {
            throw new Exception("Otrzymano nieprawidłową odpowiedź od urządzenia.");
        }

        // Wyodrębnienie danych (bajty 3, 4, 5, 6)
        byte[] dataBytes = new byte[4];
        Array.Copy(response, 3, dataBytes, 0, 4);

        // Zamiana bajtów na format C# (IEEE 754)
        return ToCSharpFloat(dataBytes);
    }
    private void WriteMultipleRegistersFloat(ushort startAddress, float value)
    {
        // Funkcja 0x10 (16) - Write Multiple Registers
        byte[] floatBytes = ToModbusFloat(value);

        byte[] data = new byte[7 + floatBytes.Length];
        data[0] = _slaveId;
        data[1] = 0x10; // Kod funkcji
        data[2] = (byte)(startAddress >> 8);   // Adres startowy Hi
        data[3] = (byte)(startAddress & 0xFF); // Adres startowy Lo
        data[4] = 0x00; // Liczba rejestrów Hi (2)
        data[5] = 0x02; // Liczba rejestrów Lo (2)
        data[6] = 0x04; // Liczba bajtów (4)
        Array.Copy(floatBytes, 0, data, 7, floatBytes.Length);

        byte[] request = AddCrc(data);
        byte[] response = ExecuteRequest(request);

        // Poprawna odpowiedź to echo: [ID][Func][AddrHi][AddrLo][CountHi][CountLo][CRC]
        if (response.Length != 8 || response[0] != _slaveId || response[1] != 0x10)
        {
            throw new Exception("Otrzymano nieprawidłową odpowiedź po zapisie danych.");
        }
    }
    private void WriteSingleRegister(ushort address, ushort value)
    {
        // Funkcja 0x06 - Write Single Register
        byte[] data = new byte[6];
        data[0] = _slaveId;
        data[1] = 0x06; // Kod funkcji
        data[2] = (byte)(address >> 8);   // Adres Hi
        data[3] = (byte)(address & 0xFF); // Adres Lo
        data[4] = (byte)(value >> 8);     // Wartość Hi
        data[5] = (byte)(value & 0xFF);   // Wartość Lo

        byte[] request = AddCrc(data);
        byte[] response = ExecuteRequest(request);

        // Poprawna odpowiedź to echo całego zapytania
        if (!request.SequenceEqual(response))
        {
            throw new Exception("Otrzymano nieprawidłową odpowiedź po zapisie rejestru.");
        }
    }
    private byte[] ExecuteRequest(byte[] requestFrame)
    {
        if (!_client.Connected)
        {
            throw new InvalidOperationException("Klient nie jest połączony. Wywołaj Connect() przed wysłaniem zapytania.");
        }

        _stream.Write(requestFrame, 0, requestFrame.Length);

        // Czekaj na dane w buforze
        Thread.Sleep(100); // Daj urządzeniu chwilę na odpowiedź

        byte[] buffer = new byte[256];
        int bytesRead = _stream.Read(buffer, 0, buffer.Length);

        byte[] response = new byte[bytesRead];
        Array.Copy(buffer, 0, response, 0, bytesRead);

        // Podstawowa walidacja odpowiedzi
        if (response.Length < 5) // Minimalna odpowiedź to [ID][Func/Err][Code][CRC_Lo][CRC_Hi]
        {
            throw new Exception("Odpowiedź od urządzenia jest za krótka.");
        }

        // Sprawdzenie sumy kontrolnej CRC
        byte[] responseData = new byte[bytesRead - 2];
        Array.Copy(response, 0, responseData, 0, bytesRead - 2);
        byte[] crc = CalculateCrc(responseData);
        if (crc[0] != response[bytesRead - 2] || crc[1] != response[bytesRead - 1])
        {
            throw new Exception("Błąd sumy kontrolnej CRC w odpowiedzi.");
        }

        // Sprawdzenie, czy urządzenie nie zgłosiło błędu Modbus (MSB kodu funkcji = 1)
        if ((response[1] & 0x80) > 0)
        {
            throw new Exception($"Urządzenie zgłosiło błąd Modbus. Kod funkcji: 0x{response[1]:X2}, kod błędu: 0x{response[2]:X2}");
        }

        return response;
    }
    private byte[] BuildRequestFrame(byte slaveId, byte functionCode, ushort startAddress, ushort count)
    {
        byte[] frame = new byte[6];
        frame[0] = slaveId;
        frame[1] = functionCode;
        frame[2] = (byte)(startAddress >> 8);
        frame[3] = (byte)(startAddress & 0xFF);
        frame[4] = (byte)(count >> 8);
        frame[5] = (byte)(count & 0xFF);
        return AddCrc(frame);
    }
    private byte[] AddCrc(byte[] data)
    {
        byte[] crc = CalculateCrc(data);
        byte[] result = new byte[data.Length + 2];
        Array.Copy(data, 0, result, 0, data.Length);
        result[data.Length] = crc[0];
        result[data.Length + 1] = crc[1];
        return result;
    }
    private static byte[] CalculateCrc(byte[] data)
    {
        ushort crc = 0xFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                bool isLsbSet = (crc & 0x0001) != 0;
                crc >>= 1;
                if (isLsbSet)
                {
                    crc ^= 0xA001;
                }
            }
        }
        return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
    }
    private float ToCSharpFloat(byte[] modbusBytes)
    {
        if (modbusBytes.Length != 4) throw new ArgumentException("Tablica musi mieć 4 bajty.");
        byte[] csharpBytes = { modbusBytes[2], modbusBytes[3], modbusBytes[0], modbusBytes[1] };
        return BitConverter.ToSingle(csharpBytes, 0);
    }
    private byte[] ToModbusFloat(float value)
    {
        byte[] csharpBytes = BitConverter.GetBytes(value);
        if (csharpBytes.Length != 4) throw new ArgumentException("Błąd konwersji float na bajty.");
        byte[] modbusBytes = { csharpBytes[2], csharpBytes[3], csharpBytes[0], csharpBytes[1] };
        return modbusBytes;
    }
    public void Dispose()
    {
        Disconnect();
        _client?.Dispose();
    }
}