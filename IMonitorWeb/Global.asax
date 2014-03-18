<%@ Application Language="C#" %>

<script runat="server">

  private static System.Threading.Timer timerMorning = null;
  private static System.Threading.Timer timerAfternoon = null;
  private static System.Threading.Timer timerSyncStore = null;
  
  private static void PrinterTask(object state)
  {
    IMonitorTask.GetPrinterTask();    
  }

  private static void SyncStoreTask(object state)
  {
    IMonitorService.Code.SqlHelper.SyncStoreInformation();
  }
  
  // 周期任务
  public static void AddTask(int seconds, Action todo)
  {
    HttpRuntime.Cache.Insert(
      Guid.NewGuid().ToString(),
      0,
      null,
      DateTime.Now.AddSeconds(seconds),
      Cache.NoSlidingExpiration,
      CacheItemPriority.NotRemovable,
      (key, value, reason) =>
      {
        todo.Invoke();
        AddTask(seconds, todo);
      });
  }    

  void Application_Start(object sender, EventArgs e) 
  {
    // 在应用程序启动时运行的代码
    //string[] print1 = IMonitorConfig.GetSetting("print1").Split(':');
    //string[] print2 = IMonitorConfig.GetSetting("print2").Split(':');
    //string router = IMonitorConfig.GetSetting("router");
    //string laptop = IMonitorConfig.GetSetting("laptop");

    //DateTime LuckTime = DateTime.Now.Date.Add(new TimeSpan(Convert.ToInt32(print1[0]), Convert.ToInt32(print1[1]), 0));  // 打印机每天获取时间为上午11点
    //DateTime GoodTime = DateTime.Now.Date.Add(new TimeSpan(Convert.ToInt32(print2[0]), Convert.ToInt32(print2[1]), 0));  // 和下午16点30分
    //DateTime SyncTime = DateTime.Now.Date.Add(new TimeSpan(9, 0, 0)); // 9点同步店铺信息
    //TimeSpan span1 = LuckTime - DateTime.Now;
    //TimeSpan span2 = GoodTime - DateTime.Now;
    //TimeSpan spanStore = SyncTime - DateTime.Now;
    
    //spanStore = (spanStore < TimeSpan.Zero) ? (SyncTime.AddDays(1d) - DateTime.Now) : spanStore;
    //span1 = (span1 < TimeSpan.Zero) ? (LuckTime.AddDays(1d) - DateTime.Now) : span1;
    //span2 = (span2 < TimeSpan.Zero) ? (GoodTime.AddDays(1d) - DateTime.Now) : span2;
    
    //object state1 = new object();
    //object state2 = new object();
    //object state3 = new object(); 
    
    //timerMorning = new System.Threading.Timer(new System.Threading.TimerCallback(PrinterTask), state1, span1, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
    //timerAfternoon = new System.Threading.Timer(new System.Threading.TimerCallback(PrinterTask), state2, span2, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
    //timerSyncStore = new System.Threading.Timer(new System.Threading.TimerCallback(SyncStoreTask), state3, spanStore, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
    
    //AddTask(Convert.ToInt32(router) * 60, IMonitorTask.GetRouterTask); // 5 分钟一次路由信息
    //AddTask(Convert.ToInt32(laptop) * 60, IMonitorTask.GetLaptopTask); // 8 分钟一次笔记本信息
    // 后续加入指纹打卡机和客流统计的自动任务
  }
    
  void Application_End(object sender, EventArgs e) 
  {
    //  在应用程序关闭时运行的代码
    if (timerMorning != null)
    {
      timerMorning.Dispose(); 
    }
    if (timerAfternoon != null)
    {
      timerAfternoon.Dispose(); 
    }
    if (timerSyncStore != null)
    {
      timerSyncStore.Dispose(); 
    }
  }
        
  void Application_Error(object sender, EventArgs e) 
  { 
      // 在出现未处理的错误时运行的代码

  }

  void Session_Start(object sender, EventArgs e) 
  {
      // 在新会话启动时运行的代码

  }

  void Session_End(object sender, EventArgs e) 
  {
      // 在会话结束时运行的代码。 
      // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
      // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
      // 或 SQLServer，则不引发该事件。

  }
       
</script>
