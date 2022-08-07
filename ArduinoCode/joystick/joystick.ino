int val=0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  int val_x = analogRead(A0);
  Serial.println("x value : " + String(val_x));

  int val_y = analogRead(A1);
  Serial.println("y value : " + String(val_y));

  int val_push = digitalRead(5);
  Serial.println("z value : " + String(val_push));
  
  delay(500);
}
