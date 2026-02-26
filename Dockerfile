FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile"]