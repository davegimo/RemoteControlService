//M101

#include <f401reMap.h>

//Pin definitions
#define SONIC_TRIGGER	2
#define SONIC_ECHO		3
#define SONIC_DELTA		20
#define SONIC_MEASURES	3l
 
//Variables for distance
long durata[SONIC_MEASURES];
long durataMedia;
long distanza;
long vecchiaDistanza;

void setup() {
	//Radio works with serial interface
	//Configured to work on 115200 baud
	//This value and mode of operation is experimental
	Serial.begin(115200);
	
	//Pin setup
	pinMode(SONIC_TRIGGER, OUTPUT);
	pinMode(SONIC_ECHO, INPUT);
	
	//LOW sonic pins
	digitalWrite(SONIC_ECHO, LOW);
	digitalWrite(SONIC_TRIGGER, LOW);
	
	//Variables init
	for (int i = 0; i < SONIC_MEASURES; i++){
		durata[i] = 0;
	}
	distanza = 0;
	vecchiaDistanza = 0;
}
 
void loop() {
	durataMedia = 0;
	
	//Have <SONIC_MEASURES> measures
	for (int i = 0;i < SONIC_MEASURES;i++){
		//Send HIGH pulse for 10 microseconds on TRIGGER pin
		digitalWrite(SONIC_TRIGGER, HIGH);
		delayMicroseconds(10);
		digitalWrite(SONIC_TRIGGER, LOW);
		
		//Read number of microseconds ECHO remains high
		//Use pulseIn()
		durata[i] = pulseIn(SONIC_ECHO, HIGH);
	}

	//Calculate a mean amongst the values
	for (int i = 0; i < SONIC_MEASURES; i++){
		durataMedia += durata[i]/SONIC_MEASURES;
	}
	
	// The speed of sound is 340 m/s, or 29 microseconds per cm.
	// Our impulse has to hit the target and bounce back. Divide by 2
	vecchiaDistanza = distanza;
	distanza = durataMedia / 29 / 2;
	
	if (distanza<3000){
		Serial.println("[DEBUG] Distanza : " + String(distanza));
	}
	
	if (vecchiaDistanza-distanza> SONIC_DELTA){
		//Note: Radio works via Serial interface (TX and RX)
		//Signals over Serial are coded as high-low digital signals
		//The radio we used is fully analogic: it will transmit the signal recieved on the input pin
		Serial.println(F("--------------------"));
		Serial.println(F("**ALERT**"));
		Serial.println("Detected movement at " + String(distanza/100) + " Meters");
		Serial.println(F("--------------------"));
	}
	delay(100);
}
