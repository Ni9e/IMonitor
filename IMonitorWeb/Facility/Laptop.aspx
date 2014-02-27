<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Laptop.aspx.cs" Inherits="Facility_Laptop" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>笔记本</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div class="row">
    <div class="col-md-12 div-center">
      <table id="tbLaptop"></table>
    </div>    
  </div>

  <script>
    var count = 0;

    $('#mfacility').addClass('active');
    $('#mlaptop').addClass('active');

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

    $('#tbLaptop').jqGrid({
      url: '/Facility/LaptopJSON.aspx?status=all',
      datatype: "json",
      colNames: ["店号", "区域", "店铺类型", "笔记本网络", "笔记本IP", "打印机服务", "获取日期"],
      colModel: [
          {
            name: "StoreNo", index: "StoreNo", width: 140, align: "center", formatter: function (cellvalue, options, rowObject) {
              if (rowObject["LaptopNetwork"] == 'Down') {
                return "<span class='btn btn-danger'></span> " + cellvalue;
              } else {
                return "<span class='btn btn-success'></span> " + cellvalue;
              }
            }
          },
          { name: "StoreRegion", index: "StoreRegion", width: 150, align: "center" },
          { name: "StoreType", index: "StoreType", width: 150, align: "center" },
          { name: "LaptopNetwork", index: "LaptopNetwork", width: 250, align: "center", cellattr: cellAttr },
          { name: "IP", index: "IP", width: 100, align:"center" },
          { name: "PrinterService", index: "PrinterService", width: 250, align: "center" },
          { name: "Date", index: "Date", width: 180, align: "center" },
      ],
      rowNum: 500,
      sortname: 'StoreNo',
      viewrecords: true,
      sortorder: "desc",
      caption: "笔记本信息",
      height: 395,
      scrollrows: true,
      gridComplete: function () {
        captionCenter('#tbLaptop');
        $('#message').text("共 " + count + " 家门店笔记本不通").fadeIn(1000);
      },
      beforeRequest: function () {
        count = 0;  
      }
    });
  </script>

  <!-- 获取笔记本信息 -->
  <script>
    // 插入获取笔记本信息的链接
    $('#mreport').after('<li id="mgetlaptop"><a href="#fakelink" onclick="getLaptopData(); return false;" style="color: #E67E22;">获取笔记本数据</a></li>');
    var info = $("#alertbar");
    var mgetlaptop = $('#mgetlaptop');
    var id;

    function clearAlert() {
      info.fadeOut(1000);
      mgetlaptop.show();
      $('#tbLaptop').trigger("reloadGrid");
    }

    function time(s) {
      if (s == 0) {
        clearAlert();
        return;
      }
      info.text("正在获取笔记本信息，大约" + s + "秒, 请勿切换页面");
      id = setTimeout("time(" + (s - 1) + ")", 1000);
    }

    function getLaptopData() {
      $.ajax({
        type: "get",
        url: "/Facility/LaptopJSON.aspx?status=laptop",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
          mgetlaptop.hide();
          info.fadeToggle(2000);
          time(30); // 预估每个门店1秒, 后期首页加入
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

