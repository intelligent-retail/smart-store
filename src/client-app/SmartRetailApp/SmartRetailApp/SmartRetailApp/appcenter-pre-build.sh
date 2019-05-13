#!/usr/bin/env bash
#
# For Xamarin, change some constants located in some class of the app.
# In this sample, suppose we have an AppConstant.cs class in shared folder with follow content:
#
# namespace Core
# {
#     public class AppConstant
#     {
#         public const string ApiUrl = "https://CMS_MyApp-Eur01.com/api";
#     }
# }
# 
# Suppose in our project exists two branches: master and develop. 
# We can release app for production API in master branch and app for test API in develop branch. 
# We just need configure this behaviour with environment variable in each branch :)
# 
# The same thing can be perform with any class of the app.
#
# AN IMPORTANT THING: FOR THIS SAMPLE YOU NEED DECLARE API_URL ENVIRONMENT VARIABLE IN APP CENTER BUILD CONFIGURATION.

if [ ! -n "$MainUrl" ]
then
    echo "You need define the MainUrl variable in App Center"
    exit
fi

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/src/client-app/SmartRetailApp/SmartRetailApp/SmartRetailApp/Models/Constant.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    echo "Updating MainUrl to $MainUrl in Constant.cs"
    sed -i '' 's#MainUrl = "[-A-Za-z0-9:_./]*"#MainUrl = "'$MainUrl'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi
