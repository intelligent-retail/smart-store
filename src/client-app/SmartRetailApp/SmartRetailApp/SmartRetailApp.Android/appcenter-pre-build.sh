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