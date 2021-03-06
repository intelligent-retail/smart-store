#!/usr/bin/env bash

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp/Models/Constant.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    sed -i '' 's#CartsApiName = "[-A-Za-z0-9:_./]*"#CartsApiName = "'$CartsApiName'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ApiKey = "[-A-Za-z0-9:_./]*"#ApiKey = "'$ApiKey'"#' $APP_CONSTANT_FILE
    sed -i '' 's#TenantName = "[-A-Za-z0-9:_./]*"#TenantName = "'$TenantName'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ClientId = "[-A-Za-z0-9:_./]*"#ClientId = "'$ClientId'"#' $APP_CONSTANT_FILE
    sed -i '' 's#PolicySignin = "[-A-Za-z0-9:_./]*"#PolicySignin = "'$PolicySignin'"#' $APP_CONSTANT_FILE
    sed -i '' 's#AppCenterKeyiOS = "[-A-Za-z0-9:_./]*"#AppCenterKeyiOS = "'$AppCenterKeyiOS'"#' $APP_CONSTANT_FILE
    sed -i '' 's#IosKeyChain = "[-A-Za-z0-9:_./]*"#IosKeyChain = "'$IosKeyChain'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi

INFOPLIST_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp.iOS/Info.plist

if [ -e "$INFOPLIST_FILE" ]
then
    sed -i '' 's#msalYourClientId#msal'$ClientId'#' $INFOPLIST_FILE

    echo "File content:"
    cat $INFOPLIST_FILE
fi
