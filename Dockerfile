FROM mcr.microsoft.com/dotnet/sdk:5.0.301-buster-slim

RUN dotnet tool install --global dotnet-affected --version 2.0.0-preview-1
ENV PATH="${PATH}:~/.dotnet/tools"

WORKDIR /workspace
ENTRYPOINT ["dotnet", "affected"]
