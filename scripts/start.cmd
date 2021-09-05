cd ..\Valuator\
start dotnet run --no-build --urls "http://localhost:5001"
start dotnet run --no-build --urls "http://localhost:5002"
cd ..\RankCalculator\
start dotnet run --no-build
start dotnet run --no-build
cd ..\EventsLogger\
start dotnet run --no-build
start dotnet run --no-build
cd ..\nginx\
start nginx