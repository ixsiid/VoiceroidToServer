# VoiceroidToServer
VOICEROID2をRESTful API化する


## インストール
VoiceroidCLI.exeとNAudio.dllをVOICEROID2のインストールフォルダ（通常はC:\Program Files(x86)\AHS\VOICEROID2\）にコピーしてください。

## 起動

``` cmd
dotnet VoiceroidToServer.dll --seed="シード値" --path="VOICEROID2のインストールフォルダ"
```

## 使い方
起動したら、http://localhost:5000/index.html にアクセスするとSwagger形式でAPIドキュメントが読めます。


