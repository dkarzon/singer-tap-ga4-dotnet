# singer-tap-ga4-dotnet

A [Singer.io](https://www.singer.io/) tap for Google Analytics Data API (GA4) written in C# (.NET 6)

*Work in progress...*

## Usage
Call the tap command with the config parameter and pass the output to a Singer Target. *Ensure dotnet 6.0 is also installed*
```
dotnet ./external/tap-ga4-dotnet/tap-ga4.dll -c ga4-config_c4k_au.json | some-singer-target
```

## Config

```json
{
  "credentialsJsonPath": "ga4-auth.json",
  "propertyId": "1234567890",
  "reports": "ga4-reports.json",
  "startDate": "2022-01-01",
  "endDate": "2022-01-31"
}
```
- `credentialsJsonPath` - Path to the json credentials file generated from Google Cloud Console
- `propertyId` - A Google Analytics GA4 property identifier (Can be obtained from the admin section of the property in Google Analytics web)
- `reports` - Path to the json file that defines the reports (Singer streams) to run, example below.
- `startDate` - The inclusive start date for the query in the format `YYYY-MM-DD`.
- `endDate` - The inclusive end date for the query in the format `YYYY-MM-DD`.

## Reports
Example report json format.
```json
[
  {
    "name": "ga4_events",
    "dimensions": [
      "date",
      "eventName",
      "pagePath",
      "sessionSource",
      "sessionMedium",
      "sessionCampaignName"
    ],
    "metrics": [
      "eventCount"
    ]
  }
]
```

## Example Dockerfile
```Dockerfile
FROM python:3.8

# Install tap-ga4
RUN curl -SL --output tap-ga4-dotnet.zip https://github.com/dkarzon/singer-tap-ga4-dotnet/releases/download/v0.1.0/tap-ga4-dotnet.zip \
    && unzip -d /external/tap-ga4-dotnet/ ./tap-ga4-dotnet.zip
RUN rm -f ./tap-ga4-dotnet.zip

# Dependencies for .NET
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ca-certificates \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Install .NET
ENV DOTNET_VERSION=6.0.6

RUN curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-x64.tar.gz \
    && dotnet_sha512='4fe090f934f0ba4e64a63dfccbac97d49b19a913f2a7d73abe85efd604ee5577cefd65d6e0dc02086e9fa28be4ce2bbaecb33ea70d022714138ed54deea58c72' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Install csv target
RUN pip install target-csv

# Copy config files
COPY ./configs/ga4-config.json ./ga4-config.json
COPY ./configs/ga4-reports.json ./ga4-reports.json
COPY ./configs/ga4-auth.json ./ga4-auth.json

CMD dotnet ./external/tap-ga4-dotnet/tap-ga4.dll -c ga4-config.json | target-csv
```


## Errors

```
Error running report ga4_events. Status(StatusCode="InvalidArgument", Detail="Requests are limited to 9 dimensions within a nested request. This request is for x dimensions.")
```


## API Resources
* https://developers.google.com/analytics/devguides/reporting/data/v1/api-schema
* https://ga-dev-tools.web.app/ga4/dimensions-metrics-explorer/