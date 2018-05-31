import serial
import time
import urllib.request
print("Program Start")
ser = serial.Serial('COM6', baudrate=115200)    # open serial port
print("Serial opened on: "+ser.name)            # check which port was really used
while True:
    line = ser.readline()
    print(line)
    if b"ALERT" in line.upper():
        contents = urllib.request.urlopen('''https://giothub.azurewebsites.net/api/get_report?deviceid=DAVE&report={"device":"DAVE","name":"","type":"sensor_update","value":{"sensor":"SW_MOV","name":"SW_MOV","type":"switch","value":"ON"}}''').read()
        time.sleep(10)
        ser.reset_input_buffer()
