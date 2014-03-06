<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Config.aspx.cs" Inherits="Facility_Config" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
  <title>配置策略</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-5">
      <div><asp:Label ID="lblprintMorning" runat="server" CssClass="btn btn-block btn-info"></asp:Label></div><br />
      <div><asp:Label ID="lblprintAfternoon" runat="server" CssClass="btn btn-block btn-info"></asp:Label></div><br />
      <div><asp:Label ID="lblrouter" runat="server" CssClass="btn btn-block btn-info"></asp:Label></div><br />
      <div><asp:Label ID="lbllaptop" runat="server" CssClass="btn btn-block btn-info"></asp:Label></div><br />
    </div>
    <div class="col-md-5">

    </div>
    <div class="col-md-1"></div>
  </div>
</asp:Content>

