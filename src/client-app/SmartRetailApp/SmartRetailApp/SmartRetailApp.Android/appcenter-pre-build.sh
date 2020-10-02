#!/usr/bin/env bash

GOOGLE_JSON_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp.Android/google-services.json

echo "PATH=" $GOOGLE_JSON_FILE

if [ -e "$GOOGLE_JSON_FILE" ]
then
    echo "Updating Google Json"
    echo "$GoogleJson" > $GOOGLE_JSON_FILE
    sed -i -e 's/\\"/'\"'/g' $GOOGLE_JSON_FILE

    echo "File content:"
    cat $GOOGLE_JSON_FILE
fi

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp/Models/Constant.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    sed -i '' 's#CartsApiName = "[-A-Za-z0-9:_./]*"#CartsApiName = "'$CartsApiName'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ApiKey = "[-A-Za-z0-9:_./]*"#ApiKey = "'$ApiKey'"#' $APP_CONSTANT_FILE
    sed -i '' 's#AppCenterKeyAndroid = "[-A-Za-z0-9:_./]*"#AppCenterKeyAndroid = "'$AppCenterKeyAndroid'"#' $APP_CONSTANT_FILE
    sed -i '' 's#TenantName = "[-A-Za-z0-9:_./]*"#TenantName = "'$TenantName'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ClientId = "[-A-Za-z0-9:_./]*"#ClientId = "'$ClientId'"#' $APP_CONSTANT_FILE
    sed -i '' 's#PolicySignin = "[-A-Za-z0-9:_./]*"#PolicySignin = "'$PolicySignin'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ListenConnectionString = "[-A-Za-z0-9:_./]*"#ListenConnectionString = "'$ListenConnectionString'"#' $APP_CONSTANT_FILE
    sed -i '' 's#NotificationHubName = "[-A-Za-z0-9:_./><]*"#NotificationHubName = "'$NotificationHubName'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi

ANDROID_MANIFEST_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp.Android/Properties/AndroidManifest.xml

echo "PATH=" $ANDROID_MANIFEST_FILE

if [ -e "$ANDROID_MANIFEST_FILE" ]
then
    sed -i '' 's#msalYourClientId#msal'$ClientId'#' $ANDROID_MANIFEST_FILE

    echo "File content:"
    cat $ANDROID_MANIFEST_FILE
fi
