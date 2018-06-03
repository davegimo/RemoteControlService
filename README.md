# RemoteControlService 

### Abstract

Remote Control Service is an accademic project developed for the Pervasive System class at Sapienza Università di Roma, Master Degree in Computer Engineering A.Y. 2017-18
The goal of the project is to provide a cheap and versatile monitoring system, in order to make your room safer.
The main advantage of our solution, compared to the existing services on the market, is the possibility to access the sensors through a Telegram chat terminal.

#### MVP
The initial goal of the project was to equip the system with two simple functionalities:

+ Send current snapshot from the camera whenever the movement sensor detects a change in the environment
+ Send current snapshot from the camera whenever the user sends a command through the Telegram chat

Those snapshots will be sent inside the Telegram chat.

In an advanced stage, we developed a Beta functionality: the user can record a video up to 30 seconds, through two commands.
This feature has been implemented and then presented for the final submission. 

## Idea
The idea was born during a talk with other colleagues; most of them, who shared a room or an apartment with other students,
agreed that there is a lack of cheap solutions to improve security in this kind of context. 

# Presentation
You can check the SlideShare presentation [here](https://www.slideshare.net/DavideGimondo/remote-control-service-99721976)

# Architecture
![Architecture](https://github.com/davegimo/RemoteControlService/blob/master/dd.png "architecture")

Whenever the sensor detects a movement, the Nucleo Board will send an HTTP Request to the Back-end.<br>
The IOT-Application will send to the Telegram chat the current snapshot.
Same thing happens when the user sends a command message to the Telegram Bot.

On the side of the Raspberry, it will send every second the current snapshot. This way the Application of our Back-End is always updated and doesn't have to send an explicit request to the Raspberry.
## Hardware 

+ Webcam
+ Raspberry
+ STM Nucleo Board
+ Movement sensor
+ Smartphone


##### Raspberry Pi 3 is connected to the webcam    &emsp; &emsp; &emsp;     STM Nucleo Board connected to movement sensors
<p float="left">
  <img src="https://github.com/davegimo/RemoteControlService/blob/master/photos/rasp2.jpeg" width="300" height="300" />
  &emsp;
  &emsp;
  <img src="https://github.com/davegimo/RemoteControlService/blob/master/photos/nucleo2.jpeg" width="300" height="300"/> 
  
</p>



## Technologies 

+ Azure IOT (Hub & Application)
+ Telegram ChatBot



<p float="left">
  <img src="https://github.com/davegimo/RemoteControlService/blob/master/photos/botfather.jpg" width="180" height="260" />
  &emsp;
  &emsp;
  <img src="https://github.com/davegimo/RemoteControlService/blob/master/photos/microsoft-azure.jpg" width="400" height="200"/> 
  
</p>

We decided to use Azure IOT because it's free and we consider it the most intuitive cloud service for this kind of applications.
The only requirement is to give the credit card credentials, even though we didn't buy any extra service for the project.

TelegramBot chat service was chosen because we consider Telegram one of the best chat systems in the market, which gives the opportunity to use its API and it is fully responsive. The creation of the bot was done directly from Telegram using BotFather. The dealing of the commands has been managed in the IOT-Application.

## Code
The code is composed by:

+ [STM Nucleo Board](https://github.com/davegimo/RemoteControlService/tree/master/Sensor%20Network)
+ [Raspberry Pi 3](https://github.com/davegimo/RemoteControlService/tree/master/Raspberry)
+ [IOT Hub & Application](https://github.com/davegimo/RemoteControlService/tree/master/IOT_Hub)
+ [Telegram Manager](https://github.com/davegimo/RemoteControlService/tree/master/Telegram)

## Telegram
Our Telegram chat overview:

Based on the payload message we get in the chat we understand what kind of snapshot we received:

+ "Eccoti servito!" in this case the user made an explicit request by sending the command inside the chat
+ "movement detected!" in this case our movement sensor has detected something

<img src="https://github.com/davegimo/RemoteControlService/blob/master/photos/telegram_foto.jpeg" width="300" height="600" />

### Commands

+ send photo : it will send the current snapshot
+ start : webcam will start recording up to 30 seconds
+ stop : webcam stops recording and sends the video to the chat


# Project Members - Contacts
+ Linkedin: [Davide Gimondo](https://www.linkedin.com/in/davegimo/)
+ Linkedin: [David Ghedalia](https://www.linkedin.com/in/david-ghedalia/)
+ Linkedin: [Marco Cuoci](https://www.linkedin.com/in/marco-cuoci-259231151/)

 ##
 ![Logo](https://github.com/davegimo/RemoteControlService/blob/master/photos/Sapienza_Universit___di_Roma-logo-C9225434E8-seeklogo.com%20(1).png "Sapienza")

