<%@ Application Language="C#" %>

<script runat="server">

  private static System.Threading.Timer timerMorning = null;
  private static System.Threading.Timer timerAfternoon = null;

  private static void PrinterTask(object state)
  {
    IMonitorTask.GetPrinterTask();
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
    DateTime LuckTime = DateTime.Now.Date.Add(new TimeSpan(11, 00, 0));  // 打印机每天获取时间为上午11点
    DateTime GoodTime = DateTime.Now.Date.Add(new TimeSpan(16, 30, 0)); // 和下午16点30分
    TimeSpan span1 = LuckTime - DateTime.Now;
    TimeSpan span2 = GoodTime - DateTime.Now;
    if (span1 < TimeSpan.Zero)
    {
      span1 = LuckTime.AddDays(1d) - DateTime.Now; 
    }
    if (span2 < TimeSpan.Zero)
    {
      span2 = GoodTime.AddDays(1d) - DateTime.Now; 
    }
    object state = new object();    
    
    timerMorning = new System.Threading.Timer(new System.Threading.TimerCallback(PrinterTask), state, span1, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
    timerAfternoon = new System.Threading.Timer(new System.Threading.TimerCallback(PrinterTask), state, span2, TimeSpan.FromTicks(TimeSpan.TicksPerDay));
    
    AddTask(5 * 60, IMonitorTask.GetRouterTask); // 5 分钟一次路由信息
    AddTask(8 * 60, IMonitorTask.GetLaptopTask); // 8 分钟一次笔记本信息
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
