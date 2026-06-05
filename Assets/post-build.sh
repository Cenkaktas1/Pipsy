#!/bin/bash
echo "IPA dosyasi App Store Connect'e yukleniyor..."
xcrun altool --upload-app -t ios -f "$UNITY_PLAYER_PATH" -u "$ITUNES_USERNAME" -p "$ITUNES_PASSWORD"