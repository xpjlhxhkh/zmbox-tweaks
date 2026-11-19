# 补丁说明（AppEntry.swf 修改 + 重签名）

适用：造梦盒子 v1.9（AIR 51.2，64 位）。请先备份 `AppEntry.swf` 和 `META-INF/`。

## 准备

- [FFDec](https://github.com/jindrapetrik/jpexs-decompiler/releases)（`ffdec-cli.jar`）
- Java 21（JRE/JDK）
- [AIR SDK](https://airsdk.harman.com/download)（含 `lib/adt.jar`；51.x 建议用 SDK Manager 下载）
- 你自己的 `AppEntry.swf`

## 1. 导出 AS3

```bat
java -jar ffdec-cli.jar -export script out_dir AppEntry.swf
```

得到 `out_dir/scripts/...`（约 190 个脚本）。

## 2. 修改脚本

### 2.1 去掉左上角 ram/fps 浮层

`AppEntry.as` 的 `initEnvSetting()` 中删除：

```as3
if(!compatibleMode)
{
   stage.addChild(new GameInfoReporter(16777215,true,true));
}
```

（也可只删 `stage.addChild(...)` 这一行。）

### 2.2 登录页三个按钮改单一“开始游戏”

`com/zmbox/view/LoginLayer.as` 构造函数中，把：

```as3
intoZm4 = new PushButton(this,110,450,"造梦西游4");
setScale(intoZm4,2.2);
intoZm5 = new PushButton(this,370,450,"造梦西游5");
setScale(intoZm5,2.2);
intoWs = new PushButton(this,630,450,"造梦无双");
setScale(intoWs,2.2);
```

改为：

```as3
intoZm4 = new PushButton(this,370,450,"开始游戏");
setScale(intoZm4,2.2);
intoZm5 = new PushButton(this,370,450,"造梦西游5");
setScale(intoZm5,2.2);
intoZm5.visible = false;
intoWs = new PushButton(this,630,450,"造梦无双");
setScale(intoWs,2.2);
intoWs.visible = false;
```

### 2.3 倍速键 1 / 1.5 / 2（可选）

`AppEntry.as` 的 `handleKeyDown` 中，把 `case 50` 改为 1.5 并新增 `case 51`：

```as3
case 49:
   if(!(stage.focus is TextField))
   {
      ApplicationFacade.getInstance().sendNotification("adjustSpeed",1);
   }
   break;
case 50:
   if(!(stage.focus is TextField))
   {
      ApplicationFacade.getInstance().sendNotification("adjustSpeed",1.5);
   }
   break;
case 51:
   if(!(stage.focus is TextField))
   {
      ApplicationFacade.getInstance().sendNotification("adjustSpeed",2);
   }
```

`com/zmbox/mediator/GameMediator.as` 中把局部变量改为 Number，避免 1.5 被截断：

```as3
var _loc4_:Number = 0;
...
curSpeed = _loc4_ = param1.getBody() as Number;
...
private var curSpeed:Number = 1;
```

> 注意：高帧率与倍速互斥是原作者设定。实测 60fps 下强行 `stage.frameRate = 60*curSpeed` 会让倍数越高人物越慢，建议保留原有限制（高帧率开启时提示“60帧不支持倍速调整”）。

### 2.4 帧率数据（可选，配合 tools/FpsLabel.cs）

`AppEntry.as` 里加一个计时器，每 500ms 把实测 FPS 写入 `%TEMP%\zmBoxFps.txt`：

```as3
import flash.filesystem.File;
import flash.filesystem.FileMode;
import flash.filesystem.FileStream;

private function onFpsTick(e:TimerEvent):void
{
   try
   {
      var f:FileStream = new FileStream();
      f.open(new File("C:/Users/<你的用户名>/AppData/Local/Temp/zmBoxFps.txt"), FileMode.WRITE);
      f.writeUTFBytes((fpsFrames * 2).toString());
      f.close();
   }
   catch (err:Error) { }
   fpsFrames = 0;
}
```

## 3. 导入并保存

```bat
java -jar ffdec-cli.jar -importScript AppEntry.swf AppEntry.mod.swf import_dir
```

`import_dir` 下需为 `scripts/...` 结构，**只放改动过的 .as**（例如 `scripts/AppEntry.as`、`scripts/com/zmbox/view/LoginLayer.as`、`scripts/com/zmbox/mediator/GameMediator.as`）。

## 4. 自签重打包

```bat
java -jar adt.jar -certificate -cn zmBoxMod -validityPeriod 10 2048-RSA cert.p12 pass123
java -jar adt.jar -package -storetype pkcs12 -keystore cert.p12 -storepass pass123 -target air out.air application.xml -C stage .
```

要点：

- `application.xml` 从原 `META-INF/AIR/application.xml` 复制；若引用的 `icons/` 不存在会报 `error 303`，删掉 `<icon>...</icon>` 块即可。
- 把 `.air` 当 zip 解压，覆盖到盒子目录的 `META-INF/` 与 `AppEntry.swf`。
- 启动器会接受自签包（校验签名完整性，不校验颁发者）。
- 备份原文件以便回滚。
