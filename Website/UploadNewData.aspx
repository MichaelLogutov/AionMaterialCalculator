<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="UploadNewData.aspx.cs" Inherits="Website.UploadNewData" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cD" runat="server">
	<div id="upload" class="fileUpload">
		Upload zip</div>
	<h2 id="progress">
	</h2>
	<script type="text/javascript">
		$(function()
		{
			var uploader = new AjaxUpload
			(
				'#upload',
				{
					action: '/AionUploadNewData.axd',
					name: 'data',
					autoSubmit: true,
					onSubmit: function(file, extension)
					{
						if (extension != "zip")
						{
							ShowMessageBox("Please select zip file");
							return false;
						}

						$("#upload").remove();
						$("#progress").text("Uploading...");
					},
					onComplete: function(file, response)
					{
						uploader.destroy();

						$.jmsajax
						({
							url: "/Aion/Ajax.asmx/ProceedNewData",
							cache: false,
							success: function(data, textStatus)
							{
								if (data.Code != 0)
								{
									ShowMessageBox(data.Data);
									return;
								}

								var id = data.Data;

								var timer = window.setInterval(function()
								{
									$.jmsajax
									({
										url: "/Aion/Ajax.asmx/GetProceedNewDataProgress",
										data: { id: id },
										cache: false,
										success: function(data, textStatus)
										{
											if (data.Code != 0)
											{
												ShowMessageBox(data.Data);
												return;
											}

											if (!data.Data)
											{
												window.clearInterval(timer);
												$("#progress").text("Complete");

												return;
											}

											$("#progress").text("Updating " + data.Data + " %");
										}
									});
								}, 1000);
							}
						});
					}
				});
		});
	</script>
</asp:Content>
