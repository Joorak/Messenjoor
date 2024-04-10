#!/bin/sh

# curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh;
# chmod +x dotnet-install.sh;
# ./Messenjoor/API/dotnet-install.sh -c 8.0 -InstallDir ./dotnet;
# ./Messenjoor/API/dotnet/dotnet --version;
# ./Messenjoor/API/dotnet/dotnet publish -c Release -o ./out/API;



curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh;
chmod +x dotnet-install.sh;
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet8;
./dotnet8/dotnet --version;
./dotnet8/dotnet publish ./WebApp/Messenjoor.csproj -c Release -o ./dist
mv ./dist/wwwroot/* ./dist
