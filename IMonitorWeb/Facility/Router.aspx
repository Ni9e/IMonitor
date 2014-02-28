<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Router.aspx.cs" Inherits="Facility_Router" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>路由器</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div class="row">
    <div class="col-md-12 div-center">
      <table id="tbRouter"></table>
    </div>    
  </div>

  <script>
    var count = 0;

    $('#mfacility').addClass('active');
    $('#mrouter').addClass('active');

    function captionCenter(gridName) {
      $(gridName).closest("div.ui-jqgrid-view")
                 .children("div.ui-jqgrid-titlebar")
                 .css("text-align", "center")
                 .children("span.ui-jqgrid-title")
                 .css("float", "none");
    };

    function cellAttr(rowId, val, rawObject, cm, rdata) {      
      if (val == "Down") {
        count++;        
      }      
    };

    $('#tbRouter').jqGrid({
      url: '/Facility/RouterJSON.aspx?status=all',
      datatype: "json",
      colNames: ["店号", "区域", "店铺类型", "路由器网络", "路由器类型", "获取日期"],
      colModel: [
          {
            name: "StoreNo", index: "StoreNo", width: 140, align: "center", formatter: function (cellvalue, options, rowObject) {
              if (rowObject["RouterNetwork"] == 'Down') {
                return "<span class='btn btn-danger'></span> " + cellvalue;
              } else {
                return "<span class='btn btn-success'></span> " + cellvalue;
              }
            }
          },
          { name: "StoreRegion", index: "StoreRegion", width: 150, align: "center" },
          { name: "StoreType", index: "StoreType", width: 150, align: "center" },
          { name: "RouterNetwork", index: "RouterNetwork", width: 250, align: "center", cellattr: cellAttr },
          { name: "RouterType", index: "RouterType", width: 250, align: "center" },
          { name: "Date", index: "Date", width: 180, align: "center" },
      ],
      rowNum: 500,
      sortname: 'StoreNo',
      viewrecords: true,
      sortorder: "desc",
      caption: "路由器信息",
      height: 395,
      scrollrows: true,
      gridComplete: function () {
        captionCenter('#tbRouter');
        $('#message').text("共 " + count + " 家门店路由器不通").fadeIn(1000);
      },
      beforeRequest: function () {
        count = 0;       
      }
    });
  </script>

  <!-- 获取路由器信息 -->
  <script>
    // 插入获取路由器信息的链接
    $('#mreport').after('<li id="mgetrouter"><a href="#fakelink" onclick="getRouterData(); return false;" style="color: #E67E22;">获取路由器数据</a></li>');
    var info = $("#alertbar");
    var mgetrouter = $('#mgetrouter');
    var id;

    function clearAlert() {
      info.fadeOut(1000);
      mgetrouter.show();
      $('#tbRouter').trigger("reloadGrid");
    }

    function time(s) {
      if (s == 0) {
        clearAlert();
        return;
      }
      info.text("正在获取路由器信息，大约" + s + "秒, 请勿切换页面");
      id = setTimeout("time(" + (s - 1) + ")", 1000);
    }

    function getRouterData() {
      $.ajax({
        type: "get",
        url: "/Facility/RouterJSON.aspx?status=router",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
          mgetrouter.hide();
          info.fadeToggle(2000);
          time(70); // 预估每个门店1秒, 后期首页加入
        },
        success: function (data, textStatus) {
          clearTimeout(id);
          info.text(data);
          setTimeout(clearAlert, 1 * 1000);
        },
        complete: function (XMLHttpRequest, textStatus) {
          //HideLoading();
        },
        error: function () {
          //请求出错处理                    
        }
      });
    }
    </script>    
</asp:Content>

