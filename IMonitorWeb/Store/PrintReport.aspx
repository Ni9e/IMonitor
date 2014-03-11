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
            <option value="0">非全年统计报表</option>
            <option value="1">全年统计报表</option>            
      </select>
    </div>    
    <div class="col-md-1">
      <a href="#fakelink" onclick="check();" class="btn btn-primary" style="width: 100px;" id="query">查询</a>
      <img src="/contents/images/load_green.GIF" style="display: none;" id="loadimg" />
    </div>
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
    $('#year').val(date.getFullYear());
    $('#month').val() // 获取select选中的值
    $('#month').selectpicker('val', month); // 设置select的值

  </script>
</asp:Content>

