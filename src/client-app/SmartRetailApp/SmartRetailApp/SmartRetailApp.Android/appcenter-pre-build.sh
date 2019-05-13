#!/usr/bin/env bash

GOOGLE_JSON_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp.Android/google-services.json

echo "PATH=" $GOOGLE_JSON_FILE

if [ -e "$GOOGLE_JSON_FILE" ]
then
    echo "Updating Google Json"
    echo "$GOOGLE_JSON" > $GOOGLE_JSON_FILE
    sed -i -e 's/\\"/'\"'/g' $GOOGLE_JSON_FILE

    echo "File content:"
    cat $GOOGLE_JSON_FILE
fi

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp/Models/Constant.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    sed -i '' 's#MainUrl = "[-A-Za-z0-9:_./]*"#MainUrl = "'$MainUrl'"#' $APP_CONSTANT_FILE
    sed -i '' 's#CartsApiName = "[-A-Za-z0-9:_./]*"#CartsApiName = "'$CartsApiName'"#' $APP_CONSTANT_FILE
    sed -i '' 's#ApiKey = "[-A-Za-z0-9:_./]*"#ApiKey = "'$ApiKey'"#' $APP_CONSTANT_FILE
    sed -i '' 's#AppCenterKeyAndroid = "[-A-Za-z0-9:_./]*"#AppCenterKeyAndroid = "'$AppCenterKeyAndroid'"#' $APP_CONSTANT_FILE
    sed -i '' 's#AppCenterKeyiOS = "[-A-Za-z0-9:_./]*"#AppCenterKeyiOS = "'$AppCenterKeyiOS'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi
