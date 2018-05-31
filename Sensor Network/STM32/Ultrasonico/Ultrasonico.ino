#include <f401reMap.h>

//Definizione dei pin
#define SONIC_TRIGGER	2
#define SONIC_ECHO		3
#define SONIC_DELTA		20
#define SONIC_MEASURES	3l
 
//variabili utilizzate per calcolare la distanza
long durata[SONIC_MEASURES];
long durataMedia;
long distanza;
long vecchiaDistanza;

void setup() {
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
	
	//Prendo <SONIC_MEASURES> misure
	for (int i = 0;i < SONIC_MEASURES;i++){
		//Invio un impulso HIGH per 10 microsecondi sul pin del trigger
		digitalWrite(SONIC_TRIGGER, HIGH);
		delayMicroseconds(10);
		digitalWrite(SONIC_TRIGGER, LOW);
		
		//ottengo il numero di microsecondi per i quali il PIN echo e' rimasto allo stato HIGH
		//per fare questo utilizzo la funzione pulseIn()
		durata[i] = pulseIn(SONIC_ECHO, HIGH);
	}

	for (int i = 0; i < SONIC_MEASURES; i++){
		durataMedia += durata[i]/SONIC_MEASURES;
	}
	
	// La velocita' del suono e' di 340 metri al secondo, o 29 microsecondi al centimetro.
	// il nostro impulso viaggia in andata e ritorno, quindi per calcoalre la distanza
	// tra il sensore e il nostro ostacolo occorre fare:
	vecchiaDistanza = distanza;
	distanza = durataMedia / 29 / 2;
	
	if (distanza<3000){
		Serial.println("[DEBUG] Distanza : " + String(distanza));
	}
	
	if (vecchiaDistanza-distanza> SONIC_DELTA){
		Serial.println(F("--------------------"));
		Serial.println(F("**ALERT**"));
		Serial.println("Detected movement at " + String(distanza/100) + " Meters");
		Serial.println(F("--------------------"));
	}
	delay(100);
}
