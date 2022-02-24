ARG DOTNET_VERSION=5.0.301-buster-slim
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

RUN dotnet tool install --global dotnet-affected
ENV PATH="${PATH}:~/.dotnet/tools"

WORKDIR /workspace
ENTRYPOINT ["dotnet", "affected"]
