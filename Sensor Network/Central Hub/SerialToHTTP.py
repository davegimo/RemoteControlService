#M101

#Notes on the communication
#The radio modules used are mapped to a virtual COM port
#The virtual COM port maps to two hardware pins on the Raspberry PI
#This functionality comes by default on most Raspberry pi boards

#The radio does not use channels for communication, not ids
#The radio effectively works in broadcast mode
#One reciever can pick up multiple transmitters

#Libraries
import serial
import time
import urllib.request

#Debug e port open
print("Program Start")
ser = serial.Serial('COM6', baudrate=115200)    # open serial port
print("Serial opened on: "+ser.name)            # check which port was really used

#Main loop
while True:
    line = ser.readline()
    print(line)                                 # debug-only

    #Check for alerts
    if b"ALERT" in line.upper():
        contents = urllib.request.urlopen('''https://giothub.azurewebsites.net/api/get_report?deviceid=DAVE&report={"device":"DAVE","name":"","type":"sensor_update","value":{"sensor":"SW_MOV","name":"SW_MOV","type":"switch","value":"ON"}}''').read()
        time.sleep(10)                          # avoid flooding
        ser.reset_input_buffer()
