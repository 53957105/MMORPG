protoc --csharp_out=./Base ./Base/*.proto
protoc --csharp_out=./Entity ./Entity/*.proto
protoc --csharp_out=./User ./User/*.proto
protoc --csharp_out=./Player ./Player/*.proto
protoc --csharp_out=./Npc ./Npc/*.proto
protoc --csharp_out=./Map ./Map/*.proto
protoc --csharp_out=./Inventory ./Inventory/*.proto
protoc --csharp_out=./Task ./Task/*.proto
protoc --csharp_out=./Character ./Character/*.proto
protoc --csharp_out=./Fight ./Fight/*.proto

pause