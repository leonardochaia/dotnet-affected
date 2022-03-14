ARG DOTNET_VERSION=5.0.301-buster-slim
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

ARG DOTNET_AFFECTED_VERSION=2.2.0

RUN dotnet tool install --global dotnet-affected --version ${DOTNET_AFFECTED_VERSION}
ENV PATH="${PATH}:~/.dotnet/tools"

WORKDIR /workspace
ENTRYPOINT ["dotnet", "affected"]
