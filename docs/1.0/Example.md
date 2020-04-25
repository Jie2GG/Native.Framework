<h2 id="index">範例目錄</h2>

*   [範例說明](#overview)
    *   [變化概要](#changelog)
    *   [重大事件](#event)
*   [項目結構](#structure)
    *   [具體實現](#achievement)
*   [一切從需求開始](#require)

<h2 id="overview">範例說明</h2>

這個<kbd>SDK 4</kbd>範例大全是由社區用戶提出的需求整合而成，當中包括許多<kbd>SDK 4</kbd>附帶的特性;比如正則事件。

從3版本至4.1(Current)版本的變化相當的大，如果你是正在使用 <kbd>SDK 3</kbd> 甚至 <kbd>SDK 2</kbd> 的用戶可能會比較迷惘和困惑，希望以下的變化概要能對你有幫助。

<h3 id="changelog">變化概要</h3>

- <kbd>SDK 4.0</kbd> 項目重新命名了(``Native.Csharp`` > ``Native.Core``),(``Native.Csharp.Sdk`` > ``Native.Sdk``),(``Native.Csharp.Tool`` > ``Native.Tool``):ok_hand:

- <kbd>SDK 4.0</kbd> ``Native.Csharp.Sdk\App\.*`` 下的文件暴露到主層級了，也就是說現在``CQMain.cs``，``Export\.*``是直接在SDK項目的最上層。:muscle:

- <kbd>SDK 4.0</kbd> ``AppInfo.cs`` 怎麼消失了？？原來是改到``SDK``項目下了(具體是在``Native.Sdk\Cqp\Model\AppInfo.cs``:heart:

- <kbd>SDK 4.0</kbd> ``Native.Csharp.Sdk\Lib\.*`` 移動到上層主目録成為公共件了喔！:ok_hand:

- <kbd>SDK 4.0</kbd> ``Repair``兼容組件終於完成使命了，很快``CoolQ Pro``就會透過多個獨立進程執行單一應用了(StandaloneProcess)，是一個酷Q副進程跑一個應用喔:star:

- <kbd>SDK 4.0</kbd> 現在 ``Native.Csharp\{appId}.json`` 終於不用因應項目名稱改動而修改了，直接是對應的``app.json``了:star:

- <kbd>SDK 4.0</kbd> 好友/群添加請求的識別flag再修改！ 從 ``e.ResponseFlag`` 更名至 ``e.Request`` 了:star:

- <kbd>SDK 4.0</kbd> ``Native.Tool`` IniConfig 新増序列化和反序列化特性 (adv)

- <kbd>SDK 1</kbd><kbd>SDK 2</kbd><kbd>SDK 3</kbd> 與舊版本不相容，若在酷Q下運行甚至放置(即不啟用)一些舊版本與<kbd>SDK 4</kbd>，酷Q會沒有回應並在``20``秒後跳出錯誤提示。**必須**把所有應用之SDK版本升級才可解決。(這個問題正在向酷Q爭取優化中！):neutral_face:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd><kbd>SDK 4.0</kbd> ``Interface`` 都放到 ``Native.Sdk``項目裏了，而[Native.Sdk](https://www.nuget.org/packages/Native.Sdk/) 可以在``Nuget``安裝了！:cupid:

- <kbd>SDK 1</kbd><kbd>SDK 2</kbd><kbd>SDK 3</kbd><kbd>SDK 4.0</kbd>  ``Native.Core.CQMain``是酷Q應用主入口類，所有事件**必須**繼承事件介面``Native.Sdk/Cqp/Interface/I*``並以應用json(``Native.Core/app.json``)文件中的相應事件屬性``name``命名註冊。註冊命名配對主要作用是針對多個相同類別事件的區分,比如事件具不一優先性或是正則特性處理各自反響之用。:star:

- <kbd>SDK 3</kbd> ``Native.Csharp\App\Event\Event_AppMain`` 更改至 ``Native.Csharp\App\CQMain`` 實現方法還是一樣喔。(具體可參照範例實現):grey_exclamation:

- <kbd>SDK 3</kbd> ``Common`` 常用類不存在了，若要從外部呼叫 ``CQApi``，除了可以傳遞屬性外還可以如<kbd>SDK 3</kbd>一樣，自行建立一個新的``Static Class`` 在酷Q启动事件儲存相應``e.CQApi``及``e.CQLog``。而``IsRunning``則是在應用啟用/停用事件下修改其狀態。(具體可參照範例實現):no_mouth:

- <kbd>SDK 3</kbd> T4 Template 從3個文件 ``LibExport.tt``,``MenuExport.tt``,``StatusExport.tt``合併至一個了在``Native.Csharp\App\Export\CQExport.tt`` 還提供運行日誌``App\Export\CQExport.log``！:+1:

- <kbd>SDK 3</kbd> ``CqMsg``消息解析類在事件屬性``e.Message.CQCodes`` 幫你解析好了，可以直接調用 :ok_hand:

- <kbd>SDK 3</kbd> ``CqCode`` 轉換是在 ``CQApi`` 下的 ``static`` 類別，需要直接調用而不是用 ``e.CQApi``下.還有最後記得``.ToString()``或``.ToSendString()``才會是```String```類別的喔 ！:grimacing:

- <kbd>SDK 3</kbd> 許多Interface合併了，以``e.SubType``區分:muscle:
  - ``IReceiveOnlineStatusMessage``,``IReceiveDiscussPrivateMessage``,``IReceiveGroupPrivateMessage``,``IReceiveFriendMessage`` 合併進 ``IPrivateMessage`` 
  - ``IReceiveGroupMemberLeave``,``IReceiveGroupMemberRemove`` 合併進 ``IGroupMemberDecrease``
  - ``IReceiveGroupMemberPass``,``IReceiveGroupMemberBeInvitee`` 合併進 ``IGroupMemberIncrease``

- <kbd>SDK 3</kbd> 個人事件實現可以與 ``Native.Csharp``項目分離了，只需要在個人項目下引用``SDK`` , ``Native.Csharp``引用個人項目即可(具體可參照範例實現):clap:

- <kbd>SDK 2</kbd> 所有Event 都改由你實現了! ``Event_AppStatus``,``Event_DiscussMessage``,``Event_FriendMessage``,``Event_GroupMessage``,``Event_OtherMessage``,``Event_UserExpand`` 被移除了啦:fearful:

- <kbd>SDK 2</kbd> 《Interface 的分離與合併,變更大事典》
  - ``Native.Csharp\App\Interface\*`` 都移動到 ``Native.Csharp.Sdk\Cqp\Interface\*`` 更進一步分離關係:heart:
  - 在线状态消息 ``Native.Csharp\App\Interface\IEvent_OtherMessage``更名並移動至 ``Native.Csharp.Sdk\Cqp\Interface\IPrivateMessage``
  - 酷Q菜單 ``Native.Csharp\App\Interface\IEvent_UserExpand`` 更名並移動至 ``Native.Csharp.Sdk\Cqp\Interface\IMenuCall``
  - 群事件 ``Native.Csharp\App\Interface\IEvent_GroupMessage`` 被分類至多個接口而部份接口又合併了多個事件(? ``IGroupAddRequest``, ``IGroupBanSpeak``, ``IGroupManageChange``, ``IGroupMemberDecrease``,``IGroupMemberIncrease``,``IGroupMessage``,``IGroupUpload`` (注意!接口內還有``e.SubType``區分):dizzy:

- <kbd>SDK 2</kbd> ``Native.Csharp\App\Core\UserExport`` 沒有了,從<kbd>SDK 3</kbd>``MenuExport.tt``到<kbd>SDK 4</kbd> 統一``Native.Csharp\App\Export\CQExport.tt``在生成事件 :eyes:

- <kbd>SDK 2</kbd> ``Native.Csharp\App\Mode\*`` 都移動到 ``Native.Csharp.Sdk\Cqp\EventArgs\*`` 更進一步分離關係:heart:

- <kbd>SDK 2</kbd> ``Native.Csharp``項目多了一個組件優化工具,解決在酷Q內還行多個C#應用分離衝突用的。:sparkles:

- <kbd>SDK 2</kbd> 訊息事件中的攔截事件傳遞參數由 ``e.Handled`` 更名至 ``e.Handler`` 了喔:sweat_drops:

- <kbd>SDK 2</kbd> 好友/群添加請求的識別flag 從 ``e.Tag`` 更名至 ``e.ResponseFlag`` 更名至 ``e.Request`` 了:heart:

- <kbd>SDK 2</kbd> 寫酷Q日誌 ``Common.CqApi.AddLoger`` 在各接收訊息事件的傳遞參數``e.CQLog``下了:sunglasses:

- <kbd>SDK 1</kbd> .NET4.0 不支持了，架構也改變太多，酷Q提供了更多接口，建議重新建設方案:sob:

<h3 id="event">重大事件</h3>

###### 以下這些事件中的錯誤在現時版本均是已經修復,其指出在該些版本中可能出現的問題

- <kbd>SDK 2</kbd> 提取群列表永遠為``null``:joy:

- <kbd>SDK 2</kbd> 取群列表時,如出現未知性別會引致發生錯誤 :scream:

- <kbd>SDK 2</kbd> 非簡體中文下取得的事件内文是亂碼 :scream:

- <kbd>SDK 2</kbd> 回覆非GBK 字符集文字時是亂碼 :scream:

- <kbd>SDK 2</kbd> 回覆時由於``Pointer``問題可能引致酷Q閃退 :exclamation:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd> 異常處理沒正確引導至酷Q錯誤事件傳遞 :scream:

- <kbd>SDK 2</kbd> 舊版本VS(特指2012)會在建置時發生錯誤 :pensive:

- <kbd>SDK 2</kbd> 自帶的UI項目會在多次打開菜單時爆炸 :boom:

- <kbd>SDK 2</kbd> 在酷Q實現多個應用時,會因為自帶的UI項目而爆炸 :boom:

- <kbd>SDK 2</kbd> ``Tool.IniObject `` 會引致酷Q閃退(StackOverThrow) :persevere:

- <kbd>SDK 2</kbd> ``Tool.HttpWebClient  `` 之 Cookies 處理會引致酷Q閃退

- <kbd>SDK 2</kbd> 正確注冊但沒觸發事件 :astonished:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd> 附加至處理序會引致酷Q爆炸 :boom:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd>懸浮窗的參數傳遞出錯會引致酷Q爆炸 :boom:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd>許可功能命名拼寫錯誤:scream_cat:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd> 無法接收語音:confounded:

- <kbd>SDK 2</kbd><kbd>SDK 3</kbd> 部份版本因``Pointer``問題引致記憶體流失:persevere:

- <kbd>SDK 3</kbd> CqApi.AddFatalError 會引致酷Q爆炸 :boom:

- <kbd>SDK 3</kbd> 組件優化工具無法處理``unmanaged`` & ``WPF``組件而引致酷Q爆炸 :boom:

- <kbd>SDK 3</kbd>群私聊消息事件和好友消息事件必須同時注冊:astonished:

- <kbd>SDK 3</kbd>``CqApi.GetFile`` 文件名是亂碼:confused:

- <kbd>SDK 4</kbd> 運算符重載導致``Stackoverthrow`` :cold_sweat:

- <kbd>SDK 4</kbd> 群成員減少事件因QQ參數判斷而引致酷Q爆炸 :boom:

- <kbd>SDK 4</kbd> 提取群列表會因QQ參數判斷而引致酷Q爆炸 :boom:

- <kbd>SDK 4</kbd> 正則事件不能有效取出KV:persevere:

- <kbd>SDK 4</kbd> 群文件上傳事件無法取出相應群號:persevere:

- <kbd>SDK 4</kbd> GroupMemberInfo 接口群成員過期時間有誤:persevere:


<h2 id="structure">項目結構</h3>

1. _<h5 id="pj_core">Core</h5>_
   ``Native.Csharp``項目的原型，核心事件處理是在[``Code``](#pj_code)項目
   * ``CQMain.cs`` 事件注冊入口
   * ``Domain`` 應用主要資源
     * ``AppData.cs`` 酷Q Api 及 酷Q Log 靜態接口實作,若核心與入口分離需自行再實現,詳細可參照[``Code``](#pj_code)項目
   * ``Export`` 事件注冊出口<details open><summary>{...}</summary>
     * ``CQExport.tt`` 生成[DllExport](https://github.com/3F/DllExport)綁定與Unity注入的腳本
       * ``CQEventExport.cs`` 各項事件類注入
       * ``CQMenuExport.cs`` 菜單注入
       * ``CQStatusExport.cs`` 懸浮窗注入
     </details>
   * ``../Lib`` 擴展資源<details open><summary>{...}</summary>
       * ``Newtonsoft.Json.dll`` - 供``CQExport.tt``用作應用``native.csharp.demo.json``文件解析
     </details>
   * ``ModuleInitializer.cs`` 文件及資源釋放，對目錄進行連結確保程式庫能正確找到(<kbd>4.1</kbd>版本己拔除)
   * ``app.json`` **核心文件** - 應用的公開資料及事件分配,提交予酷Q讀取能確保應用能有效被存取
2. _<h5 id="pj_code">Code</h5>_
   應用核心,所有事件觸發產生的任務都在這個項目實現
   * ``Action`` 執行類<details open><summary>{...}</summary>
     * ``ReceiveImage.cs`` 接收圖片,呼叫酷Q應用接口去下載訊息中的圖片存檔至 ``CoolQ\data\image`` 文件夾
     * ``ReceiveRecord.cs`` 接收語音,同理;存檔在 ``CoolQ\data\record`` 文件夾
     * ``RemoveMessage.cs`` 撤回訊息,需具備管理員身份才能對**一般會員**發出的訊息撤回而且需要酷Q<kbd>Pro</kbd>版本
     </details>
   * ``Helper`` 輔助類<details open><summary>{...}</summary>
     * ``ProcessExtensions.cs`` 酷Q是以子程序讀取和加載應用以確保更穏定運行及提供更快速的重載應用功能,但有時候我們希望找到父程序的實體
     * ``TCPHelper.cs`` 我們不能確保TCP端口是空閒可用狀態,需要透過尋找表方式找出可用端
     * ``User32.cs`` 實現發送指令到酷Q父程序或是對視窗進行監測
     </details>
   * ``Model`` 模型類<details open><summary>{...}</summary>
     * ``CQMenu.cs`` 對酷Q父程序的菜單實例化,範例提供了執行重載應用之功能調用
     * ``Friend.cs`` 好友列表模型
     * ``Notice.cs`` 群公告模型
     * ``Vip.cs`` QQ會員模型
     </details>
   * ``Module`` 模組類<details open><summary>{...}</summary>
     * ``CoolQModule.cs`` WCF服務調用到的模組,實現``API``公開調用
     * ``HomeModule.cs`` WCF服務調用到的模組,實現靜態網站建立
     </details>
   * ``Request`` 請求類(針對QQ API)
     * ``FriendRequest.cs`` 好友列表的擴展,提供更多資料包括分組及QQ会員等級
     * ``GroupRequest.cs`` 提供群公告讀取
     * ``Icon.cs`` 用戶頭像及群頭像下載
     * ``VipInfo.cs`` 提供用戶QQ會員資料讀取
   * ``Require`` 需求類(針對外部 API)
     * ``Hitokoto.cs`` [一言](https://hitokoto.cn/api) 一言指的就是一句話語，可以是動漫中的台詞，也可以是網絡上的各種小段子。
     * ``TraceMoe.cs`` [TraceMoe](https://soruly.github.io/trace.moe) 使用動畫番劇中的截圖找到更具體的番劇資料包括[AniList](https://anilist.co/)及[MyAnimeList](https://myanimelist.net/)的資料
   * ``Service`` 服務類
     * ``CoolQDataBase.cs`` 讀取酷Q內部存取的SQLite數據庫以取得歷史訊息進行處理分析
     * ``CoolQWebSocket.cs`` 提供正向WebSocket服務供其他應用透過``WebSocket``通信
     * ``CoolQZeroMq.cs`` 提供PubSub模式的ZeroMQ推送
     * ``Startup.cs`` WCF服務啟動項
   * ``Common.cs`` 公用類,實現具體API,Log及服務的定義
   * ``Event_App.cs`` 酷Q啟用/停用及初始化與結束
   * ``Event_Friend.cs`` 好友事件
   * ``Event_Group.cs`` 群組事件
   * ``Event_Menu.cs`` 應用菜單點擊事件
   * ``Event_Message.cs`` 接收QQ訊息事件
   * ``Event_Message_Kawaii.cs`` 接收QQ訊息事件(使用酷Q提供的正則分配)
   * ``Event_Message_MenuCall.cs`` 使用酷Q提供的正則對接收的QQ訊息進行指令分配
   * ``Event_Status.cs`` 懸浮窗事件,當在酷Q啟用該懸浮窗時,會每秒執行一次
3. _<h5 id="pj_database">DataBase</h5>_
   酷Q內部存取的SQLite數據庫模型
   * ``Model``  模型類<details open><summary>{...}</summary>
     * ``Cache.cs`` 緩存模型
     * ``Event.cs`` 事件模型
     * ``Log.cs`` 日誌模型
     </details>
   * ``SQLite.cs`` [praeclarum/sqlite-net](https://github.com/praeclarum/sqlite-net) 能直接取用酷Q本身原有的SQLite組件``CoolQ\bin\sqlite3.dll``
   * ``SQLiteAsync.cs`` 
4. _<h5 id="pj_ui">UI</h5>_
   * ``AppSetting.cs`` 實現界面數劇的存取
   * ``MainForm.*`` WinForm界面實現
   * ``MainWindow.*`` WPF界面實現,提供基本訊息顯示收發功能
   * ``Data`` 數據
     * ``MainData.cs`` 主要數據
   * ``Model`` 模型<details open><summary>{...}</summary>
     * ``DesignViewModel.cs`` 設計模式下的實作模型
     * ``Group.cs`` 群組數據模型
     * ``Message.cs`` 訊息數據模型
     * ``ViewModel.cs``生產模式下的實作模型
     </details>
5. _<h5 id="pj_extension">Native.Sdk.Extension</h5>_
   * ``ApiExtention.cs`` 實作對事件接口的優化

<h2 id="achievement">具體實現</h2>

**調用QQ API** 
- [x] 好友列表
- [x] 會員信息
- [x] 頭像
- [x] 取群公告
- [ ] 群文件下載[gu]

**調用外部API**
- [x] 一言
- [x] TraceMoe

**調用酷QAPI**
- [x] 自動同意加好友
- [x] 保存消息中的圖片到本地
- [x] 正則事件

**調用WINAPI**
- [x] 指令重載應用

**建立API服務,實現其他程序調用**
- [x] Wcf訪問
- [x] CoolQ HTTP API
- [x] WebSocket

**Wpf介面數據綁定與調用酷QAPI**
- [x] 群信息綁定
- [x] 調用酷QAPI讀取群列表

**菜單調用**
- [x] 調用WPF視窗
- [x] 調用WinForm

**群管理**
- [x] 加入群提示

**自定懸浮窗**
- [x] 加好友數顯示
- [x] 使用其定時觸發實現定時事件

**SQLite 酷Q日誌存取,數據存取**
- [x] 讀取酷Q事件日誌中的訊息ID撤回信息

**Tool.IniObject**
- [x] UI設置讀寫

<h2 id="require">一切從需求開始</h2>

[New issue](https://github.com/Jie2GG/Native.Csharp.Frame/issues/new)

###### Q:怎樣發送圖片?

**A:** 需要酷Q<kbd>Pro</kbd>版本以及圖片必須在酷Q的圖片文件夾下 ``CoolQ\data\image`` (支持下層文件夾)

假設你有將圖片放在 ``CoolQ\data\image\com.jie2gg.mylove`` 下,例如命名為 ``Jie2GG.jpg``

```cs
public void GroupMessage(object sender, CQGroupMessageEventArgs e)
{
   if(e.CQApi.IsAllowSendImage) //判斷可否發送圖片(需要酷Q Pro版本)
   {
      e.CQApi.SendGroupMessage(e.FromGroup,CQApi.CQCode_Image("com.jie2gg.mylove\Jie2GG.jpg"),"(秘)");
   }
}

```