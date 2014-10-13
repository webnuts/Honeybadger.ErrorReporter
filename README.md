# Honeybadger.ErrorReporter

## Introduction

Honeybadger.ErrorReporter is a simple error reporter for Honeybadger. It collects information about exceptions,
and submits them to Honeybadger using the public API.

## Installation

Install using NuGet. Search for Honeybadger.ErrorReporter.

## Getting started

### 1. Add settings to AppSettings

You need to add two settings to your Web.config file:

1. honeybadger-api-key. You can find the API Key from your Honeybadger account. Each of your projects have a unique API key.
2. honeybadger-environment-name. Set that to whatever environments you have. For example, you might call it "development" locally, and "production" in the production environment.

Example:
```xml
<configuration>
	<appSettings>
		<add key="honeybadger-api-key" value="..."/>
		<add key="honeybadger-environment-name" value="development"/>
	</appSettings>
</configuration>
```

### 2. Caching all the errors

Add ErrorRegistrationModule to your web.config file:

```xml
<configuration>
    <system.web>
        <httpModules>
            <add name="HoneybadgerErrorRegistrationModule" type="Honeybadger.ErrorReporter.HoneybadgerErrorRegistrationModule, Honeybadger.ErrorReporter" />
        </httpModules>
    </system.web>

    <system.webServer>
        <modules runAllManagedModulesForAllRequests="true">
            <add name="HoneybadgerErrorRegistrationModule" type="Honeybadger.ErrorReporter.HoneybadgerErrorRegistrationModule, Honeybadger.ErrorReporter" />
        </modules>
    </system.webServer>
</configuration>
```