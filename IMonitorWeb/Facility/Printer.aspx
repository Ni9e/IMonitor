<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Printer.aspx.cs" Inherits="Facility_Printer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>打印机</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">  
  <div class="row">
    <div class="col-md-12 div-center">
      <table id="tbUP"></table>
    </div>    
  </div>
  <br />
  <div class="row">
    <div class="col-md-12 div-center">
      <table id="tbDown"></table>
    </div>    
  </div>

  <script>
    var sty = {},
        count = 0; // 缺墨门店统计

    $('#mfacility').addClass('active');
    $('#mprint').addClass('active');
    $('#mgetprint').show();

    function currentNum(str) {
      var arr = str.match(/\d{1,}/);
      if (arr != null) {
        return arr[0];
      }
      return null;
    };

    function captionCenter(gridName) {
      $(gridName).closest("div.ui-jqgrid-view")
                 .children("div.ui-jqgrid-titlebar")
                 .css("text-align", "center")
                 .children("span.ui-jqgrid-title")
                 .css("float", "none");
    };

    function cellAttr(rowId, val, rawObject, cm, rdata) {
      var str = currentNum(val);
      if (str != null) {
        if (parseInt(str) <= 10) {
          count++;
          sty[rowId] = "background-color:#EC7063; font-weight:bold; text-align:center;";
          return "style='background-color:#EC7063; font-weight:bold'";
        }
      }
    };

    $('#tbUP').jqGrid({
      url: '/Facility/PrinterJSON.aspx?status=up',
      datatype: "json",
      colNames: ["店号", "区域", "店铺类型", "打印机信息", "墨盒信息", "打印机类型", "墨盒类型", "日期", "网络状态"],
      colModel: [
          { name: "StoreNo", index: "StoreNo", width: 55, align: "center" },
          { name: "StoreRegion", index: "StoreRegion", width: 50, align: "center" },
          { name: "StoreType", index: "StoreType", width: 55, align: "center" },
          { name: "PrinterStatus", index: "PrinterStatus", width: 240, align: "center" },
          { name: "TonerStatus", index: "TonerStatus", width: 240, align: "center", cellattr: cellAttr },
          { name: "PrinterType", index: "PrinterType", width: 260, align: "center" },
          { name: "TonerType", index: "TonerType", width: 100, align: "center" },
          { name: "Date", index: "Date", width: 140, align: "center" },
          { name: "PrinterNetwork", index: "PrinterNetwork", width: 60, align: "center" },
      ],
      rowNum: 500,
      sortname: 'StoreNo',
      viewrecords: true,
      sortorder: "desc",
      caption: "在线打印机",
      height: 395,
      gridComplete: function () {
        for (var p in sty) {
          $('#' + p).children().each(function () { $(this).attr('style', sty[p]); });
        }
        captionCenter('#tbUP');
        $('#message').text("共 " + count + " 家门店打印机墨盒低于10%").fadeIn(1000);
      },
      beforeRequest: function () {
        count = 0; // 缺墨统计清0          
      }
    });

    $('#tbDown').jqGrid({
      url: '/Facility/PrinterJSON.aspx?status=down',
      datatype: "json",
      colNames: ["店号", "区域", "店铺类型", "打印机信息", "墨盒信息", "打印机类型", "墨盒类型", "日期", "网络状态"],
      colModel: [
          { name: "StoreNo", index: "StoreNo", width: 55, align: "center" },
          { name: "StoreRegion", index: "StoreRegion", width: 50, align: "center" },
          { name: "StoreType", index: "StoreType", width: 55, align: "center" },
          { name: "PrinterStatus", index: "PrinterStatus", width: 240, align: "center" },
          { name: "TonerStatus", index: "TonerStatus", width: 240, align: "center", cellattr: cellAttr },
          { name: "PrinterType", index: "PrinterType", width: 260, align: "center" },
          { name: "TonerType", index: "TonerType", width: 100, align: "center" },
          { name: "Date", index: "Date", width: 140, align: "center" },
          { name: "PrinterNetwork", index: "PrinterNetwork", width: 60, align: "center" },
      ],
      rowNum: 500,
      sortname: 'StoreNo',
      viewrecords: true,
      sortorder: "desc",
      caption: "离线打印机",
      height: 395,
      gridComplete: function () {        
        captionCenter('#tbDown');        
      }
    });

  </script>

  <!-- 获取打印机信息 -->
  <script>
    // 插入获取打印机信息的链接
    $('#mreport').after('<li id="mgetprint"><a href="#fakelink" onclick="getPrintData(); return false;" style="color: #E67E22;">获取打印机数据</a></li>');
    var info = $("#alertbar");
    var mgetprint = $('#mgetprint');
    var id;

    function clearAlert() {
      info.fadeOut(1000);
      mgetprint.show();
      $('#tbUP').trigger("reloadGrid");
      $('#tbDown').trigger("reloadGrid");
      sendEmail();
    }

    function time(s) {
      if (s == 0) {
        clearAlert();
        return;
      }
      info.text("正在获取打印机信息，大约" + s + "秒, 请勿切换页面");
      id = setTimeout("time(" + (s - 1) + ")", 1000);      
    }

    function getPrintData() {
      $.ajax({
        type: "get",
        url: "/Facility/PrinterJSON.aspx?status=print",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
          mgetprint.hide();
          info.fadeToggle(2000);          
          time(250); // 预估每个门店1秒, 后期首页加入
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

    function sendEmail() {
      $.ajax({
        type: "get",
        url: "/Facility/PrinterJSON.aspx?status=sendemail",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
          mgetprint.hide();
          info.fadeToggle(2000);
          info.text("正在发送邮件...");
        },
        success: function (data, textStatus) {          
          info.text("邮件发送完毕！");
          info.fadeToggle(1000);
          mgetprint.show();
          var msgtemp = $('#message').text();
          $('#message').text(msgtemp + "      " + data.substring(0, data.indexOf("<!DOCTYPE html>")));
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

