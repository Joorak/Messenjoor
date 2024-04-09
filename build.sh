#!/bin/sh

# curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh;
# chmod +x dotnet-install.sh;
# ./Messenjoor/API/dotnet-install.sh -c 8.0 -InstallDir ./dotnet;
# ./Messenjoor/API/dotnet/dotnet --version;
# ./Messenjoor/API/dotnet/dotnet publish -c Release -o ./out/API;



curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh;
chmod +x dotnet-install.sh;
./dotnet-install.sh -c 7.0 -InstallDir ./WebApp/dotnet7;
./WebApp/dotnet7/dotnet --version;
./WebApp/dotnet7/dotnet publish -c Release -o ./dist
