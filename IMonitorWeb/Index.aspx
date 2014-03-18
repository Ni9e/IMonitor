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
      <a href="#fakelink" onclick="check();" class="btn btn-primary" style="width: 100px;" id="query">查询</a>
      <img src="contents/images/load_green.GIF" style="display: none;" id="loadimg" />
    </div>
  </div>
  <br />
  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-5">
      <canvas id="myCanvas" width="560" height="225" style="border:1px solid #c3c3c3; position:absolute;">
        Your browser does not support the canvas element.
      </canvas>
    </div>
    <div class="col-md-5">
      <a href="#fakelink" class="btn btn-block btn-danger" id="routerNetwork">路由器网络: </a>
      <a href="#fakelink" class="btn btn-block btn-danger" id="printerNetwork">打印机网络: </a>
      <a href="#fakelink" class="btn btn-block btn-danger" id="laptopNetwork">笔记本网络: </a> 
      <a href="#fakelink" class="btn btn-block btn-danger" id="printerStatus">打印机状态: </a>
      <a href="#fakelink" class="btn btn-block btn-danger" id="tonerStatus">墨盒状态: </a>          
    </div>    
    <div class="col-md-1"></div>
  </div>
  <br />
  <div class="row">
    <div class="col-md-1"></div>
    <div class="col-md-5">
      <div id='pieRouter'></div>
    </div>
    <div class="col-md-5">
      <div id="piePrinter"></div>
    </div>
    <div class="col-md-1"></div>
  </div>

  <script>
    $('#mindex').addClass('active');

    var flag = 0;

    $('#storeNo').focus();
    $('#storeNo').bind("keydown", function (e) {
      var key = e.which;
      if (key == 13 && flag == 0) {
        flag = 1;
        $('#query').click();
      }
    })

    function renderPiePrinter() {
      var piedata = [];
      var total = 0;      
      $.ajax({
        type: "get",
        url: "/Facility/PrinterJSON.aspx?status=amount",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
        },
        success: function (data, textStatus) {
          piedata = eval(data);
          total = parseInt(piedata[0].Value) + parseInt(piedata[1].Value) + parseInt(piedata[2].Value);
          $(function () {
            var datas = [
                        { name: piedata[0].Name, value: piedata[0].Percent, color: piedata[0].Color },
                        { name: piedata[1].Name, value: piedata[1].Percent, color: piedata[1].Color },
                        { name: piedata[2].Name, value: piedata[2].Percent, color: piedata[2].Color }
            ];

            new iChart.Pie2D({
              render: 'piePrinter',
              data: datas,
              title: '打印机信息' + "(" + total + "家店)",
              animation: true,
              legend: {
                enable: true,
                valign: 'top',
                listeners: {
                  parse: function (t, text, i) {
                    switch (text) {
                      case "正常":
                        return { text: piedata[0].Name + " " + piedata[0].Value + " 家店" };
                        break;
                      case "缺墨":
                        return { text: piedata[1].Name + " " + piedata[1].Value + " 家店" };
                        break;
                      case "异常":
                        return { text: piedata[2].Name + " " + piedata[2].Value + " 家店" };
                        break;
                    }
                  }
                }
              },
              showpercent: true,
              decimalsnum: 2,
              width: 560,
              height: 300,
              radius: 100,
              listeners: {
                bound: function () {
                  window.location.href = '/Facility/Printer.aspx';
                },
              },
              tip: {
                enable: true,
                showType: "fixed",
                listeners: {
                  parseText: function (tip, name, value, text, index) {
                    switch (name) {
                      case "正常":
                        return piedata[0].Value + "家店";
                        break;
                      case "缺墨":
                        return piedata[1].Value + "家店";
                        break;
                      case "异常":
                        return piedata[2].Value + "家店";
                        break;
                    }
                  }
                }
              }
            }).draw();
          });
        },
        complete: function (XMLHttpRequest, textStatus) {
          //HideLoading();
        },
        error: function () {
          //请求出错处理
        }
      });
    }

    function renderPieRouter() {
      var piedata = [];
      var total = 0;
      
      $.ajax({
        type: "get",
        url: "/Facility/RouterJSON.aspx?status=amount",
        beforeSend: function (XMLHttpRequest) {
          //ShowLoading();
        },
        success: function (data, textStatus) {
          piedata = eval(data);
          total = parseInt(piedata[0].Value) + parseInt(piedata[1].Value);
          $(function () {
            var datas = [
                        { name: piedata[0].Name, value: piedata[0].Percent, color: piedata[0].Color },
                        { name: piedata[1].Name, value: piedata[1].Percent, color: piedata[1].Color }
            ];

            new iChart.Pie2D({
              render: 'pieRouter',
              data: datas,
              title: '路由器信息' + "(" + total + "家店)",
              animation: true,
              legend: {
                enable: true,
                valign: "top",
                listeners: {
                  parse: function (t, text, i) {
                    switch (text) {
                      case "正常":
                        return { text: piedata[0].Name + " " + piedata[0].Value + " 家店" };
                        break;
                      case "异常":
                        return { text: piedata[1].Name + " " + piedata[1].Value + " 家店" };
                        break;
                    }
                  }
                }
              },
              showpercent: true,
              decimalsnum: 2,
              width: 560,
              height: 300,
              radius: 100,
              listeners: {
                bound: function () {
                  window.location.href = '/Facility/Router.aspx';
                },
              },
              tip: {
                enable: true,
                showType: "fixed",
                listeners: {
                  parseText: function (tip, name, value, text, index) {
                    switch (name) {
                      case "正常":
                        return piedata[0].Value + "家店";
                        break;
                      case "异常":
                        return piedata[1].Value + "家店";
                        break;
                    }
                  }
                }
              }
            }).draw();
          });
        },
        complete: function (XMLHttpRequest, textStatus) {
          //HideLoading();
        },
        error: function () {
          //请求出错处理
        }
      });
    }
    

    setTimeout("renderPieRouter()", 1000);
    setTimeout("renderPiePrinter()", 1500);

    setInterval("renderPieRouter()", 2 * 60 * 1000);
  </script>

  <!-- 获取查询信息 -->
  <script>
    var info = $("#alertbar");
    var query = $('#query');
    var loadimg = $('#loadimg');
    var queryData = [];
    var routerInfo = { "Up": "路由器网络: UP", "Down": "路由器网络: Down" };
    var printerInfo = { "Up": "打印机网络: UP", "Down": "打印机网络: Down" };
    var laptopInfo = { "Up": "笔记本网络: UP", "Down": "笔记本网络: Down" };
    var rNetwork,
        pNetwork,
        pStatus,
        tStatus,
        lNetwork,
        ptype,
        ttype;

    function currentNum(str) {
      var arr = str.match(/\d{1,}/);
      if (arr != null) {
        return arr[0];
      }
      return null;
    };

    function check() {
      var a = $('#storeNo').val();
      var reg = /^\d{1,10}$/;
      if (reg.test(a)) {
        getQueryData();
        return true;
      }
      else {
        alert("请输入正确的店号！(0到9的数字)");
        return false;
      }
    }

    function getQueryData() {
      $.ajax({
        type: "get",
        url: "/IndexJSON.aspx?status=query&storeNo="+$('#storeNo').val(),
        beforeSend: function (XMLHttpRequest) {          
          query.hide();
          loadimg.show();
          $('#routerNetwork').removeClass("btn-success").addClass("btn-danger").text("").text("路由器网络: ");
          $('#printerNetwork').removeClass("btn-success").addClass("btn-danger").text("").text("打印机网络: ");
          $('#printerStatus').removeClass("btn-success").addClass("btn-danger").text("").text("打印机状态: ");
          $('#tonerStatus').removeClass("btn-success").removeClass("btn-warning").addClass("btn-danger").text("").text("墨盒状态: ");
          $('#laptopNetwork').removeClass("btn-success").addClass("btn-danger").text("").text("笔记本网络: ");

          setCanvas("Down", "Down", "Down", "");
        },
        success: function (data, textStatus) {
          flag = 0;
          loadimg.hide();
          query.show();
          queryData = eval(data);
          rNetwork = queryData[0]["RouterNetwork"];
          pNetwork = queryData[0]["PrinterNetwork"];
          pStatus = queryData[0]["PrinterStatus"];
          tStatus = queryData[0]["TonerStatus"];
          lNetwork = queryData[0]["LaptopNetwork"];
          ptype = queryData[0]["PrinterType"];
          ttype = queryData[0]["TonerType"];

          $('#routerNetwork').text(routerInfo[rNetwork]);
          $('#printerNetwork').text(printerInfo[pNetwork] + "~~[" + ptype + "]");
          $('#printerStatus').text($('#printerStatus').text() + pStatus);
          $('#tonerStatus').text($('#tonerStatus').text() + tStatus + "~~[" + ttype + "]");
          $('#laptopNetwork').text(laptopInfo[lNetwork]);
          if (rNetwork == "Up") {
            $('#routerNetwork').removeClass("btn-danger").addClass("btn-success");
          }
          if (pNetwork == "Up") {
            $('#printerNetwork').removeClass("btn-danger").addClass("btn-success");
          } 
          if (pStatus != "" && pNetwork == "Up") {
            $('#printerStatus').removeClass("btn-danger").addClass("btn-success");
          }
          if (tStatus != "" && pNetwork == "Up") {
            var str = currentNum(tStatus);
            if (parseInt(str) <= 10) {
              $('#tonerStatus').removeClass("btn-danger").addClass("btn-warning");
            } else {
              $('#tonerStatus').removeClass("btn-danger").addClass("btn-success");
            }            
          }
          if (lNetwork == "Up") {
            $('#laptopNetwork').removeClass("btn-danger").addClass("btn-success");
          }
          setCanvas(rNetwork, pNetwork, lNetwork, tStatus);
        },
        complete: function (XMLHttpRequest, textStatus) {
          //HideLoading();
        },
        error: function () {
          //请求出错处理                    
        }
      });
    }

    function setCanvas(r, p, l, t) {
      var c = document.getElementById("myCanvas");
      var cxt = c.getContext("2d");
      var img1 = new Image();

      if (r == "Up") {
        img1.src = "/contents/images/lyq.png";
      } else {
        img1.src = "/contents/images/lyq_b.png";
      }      
      img1.onload = function () {
        cxt.drawImage(img1, 15, 45, 140, 120);
      };

      var img2 = new Image();      
      var tstatus = currentNum(t);
      
      if (p == "Up" && parseInt(tstatus) <= 10) {
        img2.src = "/contents/images/dyj_l.png";
      } else if (p == "Up") {
        img2.src = "/contents/images/dyj.png";
      } else if (p == "Down") {
        img2.src = "/contents/images/dyj_b.png";
      }
      img2.onload = function () {
        cxt.drawImage(img2, 215, 48, 140, 120);
      };

      var img3 = new Image();
      if (l == "Up") {
        img3.src = "/contents/images/bjb.png";
      } else {
        img3.src = "/contents/images/bjb_b.png";
      }      
      img3.onload = function () {
        cxt.drawImage(img3, 415, 50, 140, 120);
      };
    }

    setCanvas("Down", "Down", "Down", "");
  </script> 
</asp:Content>

