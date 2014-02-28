<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Index.aspx.cs" Inherits="Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>主页</title>
  <script src="/contents/js/ichart.1.2.min.js"></script>
  <script src="/contents/js/ichartjs.base.js"></script>
  <script src="/contents/js/ichartjs.pie.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-2">
      <input type="text" class="form-control" style="width: 200px;" placeholder="输入店号" id="storeNo" /> 
    </div>
    <div class="col-md-1">
      <a href="#fakelink" class="btn btn-primary" style="width: 100px;">查询</a>
    </div>
  </div>

  <div class="row">
    <div class="col-md-6">
            
    </div>
    <div class="col-md-6">

    </div>
  </div>
  <div class="row">
    <div class="col-md-6">
      <div id='pieRouter' class="div-center"></div>
    </div>
    <div class="col-md-6">
      <div id="piePrinter" class="div-center"></div>
    </div>
  </div>


  <script>
    $('#mindex').addClass('active');

  </script>
</asp:Content>

