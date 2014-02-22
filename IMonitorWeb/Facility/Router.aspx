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
      }
    });
  </script>
</asp:Content>

