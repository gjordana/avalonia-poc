#!/bin/bash
set -e

APP_NAME="TodoApp"
BUNDLE_ID="com.todoapp.avalonia"
PUBLISH_DIR="publish"
APP_BUNDLE="$PUBLISH_DIR/$APP_NAME.app"
RID="osx-arm64"

echo "=== Publicando $APP_NAME para macOS arm64 (self-contained) ==="

# Clean
rm -rf "$PUBLISH_DIR"

# Publish self-contained single-file
dotnet publish \
    -c Release \
    -r $RID \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -o "$PUBLISH_DIR/bin"

echo "=== Creando bundle .app ==="

# Create .app structure
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy published files
cp -R "$PUBLISH_DIR/bin/"* "$APP_BUNDLE/Contents/MacOS/"

# Make executable
chmod +x "$APP_BUNDLE/Contents/MacOS/$APP_NAME"

# Create Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>TodoApp - Gestión de Tareas</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
</dict>
</plist>
PLIST

# Generate .icns icon
echo "=== Generando ícono ==="
ICONSET_DIR="/tmp/${APP_NAME}.iconset"
rm -rf "$ICONSET_DIR"
mkdir -p "$ICONSET_DIR"

# Use the app's PNG asset to generate all required sizes
SOURCE_ICON="Assets/avalonia-logo.png"
if [ -f "$SOURCE_ICON" ]; then
    sips -z 16 16     "$SOURCE_ICON" --out "$ICONSET_DIR/icon_16x16.png"      > /dev/null 2>&1
    sips -z 32 32     "$SOURCE_ICON" --out "$ICONSET_DIR/icon_16x16@2x.png"   > /dev/null 2>&1
    sips -z 32 32     "$SOURCE_ICON" --out "$ICONSET_DIR/icon_32x32.png"      > /dev/null 2>&1
    sips -z 64 64     "$SOURCE_ICON" --out "$ICONSET_DIR/icon_32x32@2x.png"   > /dev/null 2>&1
    sips -z 128 128   "$SOURCE_ICON" --out "$ICONSET_DIR/icon_128x128.png"    > /dev/null 2>&1
    sips -z 256 256   "$SOURCE_ICON" --out "$ICONSET_DIR/icon_128x128@2x.png" > /dev/null 2>&1
    sips -z 256 256   "$SOURCE_ICON" --out "$ICONSET_DIR/icon_256x256.png"    > /dev/null 2>&1
    sips -z 512 512   "$SOURCE_ICON" --out "$ICONSET_DIR/icon_256x256@2x.png" > /dev/null 2>&1
    sips -z 512 512   "$SOURCE_ICON" --out "$ICONSET_DIR/icon_512x512.png"    > /dev/null 2>&1
    sips -z 1024 1024 "$SOURCE_ICON" --out "$ICONSET_DIR/icon_512x512@2x.png" > /dev/null 2>&1
    iconutil -c icns "$ICONSET_DIR" -o "$APP_BUNDLE/Contents/Resources/AppIcon.icns"
    echo "  ícono generado OK"
else
    echo "  AVISO: no se encontró $SOURCE_ICON, el .app no tendrá ícono"
fi
rm -rf "$ICONSET_DIR"

# Clean up intermediate publish dir
rm -rf "$PUBLISH_DIR/bin"

# Calculate size
SIZE=$(du -sh "$APP_BUNDLE" | cut -f1)

echo ""
echo "=== Publicación completada ==="
echo "  Archivo: $APP_BUNDLE"
echo "  Tamaño:  $SIZE"
echo "  Runtime: .NET incluido (self-contained)"
echo ""
echo "Para instalar, copiá $APP_BUNDLE a /Applications/"
echo "  cp -R $APP_BUNDLE /Applications/"
