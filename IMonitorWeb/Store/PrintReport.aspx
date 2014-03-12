<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="PrintReport.aspx.cs" Inherits="Store_PrintReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>墨盒统计报表</title>
  <script src="/contents/js/bootstrap-select.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-2">
      <input type="text" class="form-control" style="width: 200px;" placeholder="年" id="year" />
    </div>
    <div class="col-md-2">
      <select id="month" value="" class="selectpicker">
            <option value="01">01</option>
            <option value="02">02</option>
            <option value="03">03</option>
            <option value="04">04</option>
            <option value="05">05</option>
            <option value="06">06</option>
            <option value="07">07</option>
            <option value="08">08</option>
            <option value="09">09</option>
            <option value="10">10</option>
            <option value="11">11</option>
            <option value="12">12</option>
      </select>
    </div>    
    <div class="col-md-2">
      <select id="current" value="0" class="selectpicker">
            <option value="0">每月统计</option>
            <option value="1">全年统计</option>            
      </select>
    </div>    
    <div class="col-md-1">
      <a href="#fakelink" onclick="getReport();" class="btn btn-primary" style="width: 100px;" id="query">查询</a>
      <img src="/contents/images/load_green.GIF" style="display: none;" id="loadimg" />
    </div>
  </div>

  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-10">
      <table id="tbReport"></table>
    </div>
    <div class="col-md-1"></div>
  </div>
  <script>
    $('#mreport').addClass('active');
    $('#mprintrepo').addClass('active');
    $('.selectpicker').selectpicker({
      style: 'btn-primary',
      size: 3
    });

    var date = new Date();
    var month = date.getMonth() + 1;
    if (month >= 1 && month <= 9) {
      month = "0" + month;
    } else {
      month += '';
    }
    $('#year').val(date.getFullYear()); // 初始化当前年  
    $('#month').selectpicker('val', month); // 初始化当前月

    function captionCenter(gridName) {
      $(gridName).closest("div.ui-jqgrid-view")
                 .children("div.ui-jqgrid-titlebar")
                 .css("text-align", "center")
                 .children("span.ui-jqgrid-title")
                 .css("float", "none");
    };
       

    function getReport() {
      var y = $('#year').val();
      var m = $('#month').val();
      var c = $('#current').val();
      var url = "/Store/PrintReportJSON.aspx?status=query&year=" + y + "&month=" + m + "&isyear=" + c;     

      $('#tbReport').jqGrid({
        url: url,
        datatype: "json",
        colNames: ["店号", "区域", "店铺类型", "墨盒使用数量", "店铺状态", "日期"],
        colModel: [
            { name: "StoreNo", index: "StoreNo", width: 150, align: "center" },
            { name: "StoreRegion", index: "StoreRegion", width: 150, align: "center" },
            { name: "StoreType", index: "StoreType", width: 150, align: "center" },
            { name: "TonerCount", index: "TonerCount", width: 250, align: "center" },
            { name: "StoreStatus", index: "StoreStatus", width: 250, align: "center" },
            { name: "Date", index: "Date", width: 200, align: "center" },
        ],
        rowNum: 500,
        sortname: 'StoreNo',
        viewrecords: true,
        sortorder: "desc",
        caption: "门店墨盒使用统计",
        height: 395,
        scrollrows: true,
        gridComplete: function () {
          captionCenter('#tbReport');
          $('#loadimg').hide();
          $('#query').show();
        },
        beforeRequest: function () {
          $('#query').hide();
          $('#loadimg').show();
        }
      });

      $("#tbReport").jqGrid('setGridParam', { url: url }).trigger("reloadGrid");
    }    
        
  </script>
</asp:Content>

