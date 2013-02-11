<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Website.Default" EnableViewState="false" %>

<%@ Import Namespace="Resources" %>

<%--<%@ OutputCache CacheProfile="Long" VaryByParam="lang" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="cD" runat="server">
    <div id="RecipeCalculator_aspx">
        <table style="width: 100%">
            <tr>
                <td>
                    <input class="addRecipe" type="button" value="<%= RecipeCalculator.s115 %>" style="height: 30px" />
                </td>
                <td class="topMenu">
                    <asp:Literal runat="server" ID="langLinks" Mode="PassThrough"></asp:Literal>
                    <span style="padding-left: 50px"></span><a href="javascript:;" class="reset" tooltip="<%= RecipeCalculator.s113 %>">
                        <%= RecipeCalculator.s114 %></a> | <a href="<%= this.Request.RawUrl %>" tooltip="<%= RecipeCalculator.s38 %>">
                            <%= RecipeCalculator.s39 %></a> | <a href="javascript:;" class="feedback" tooltip="<%= RecipeCalculator.s34 %>">
                                <%= RecipeCalculator.s35 %></a> | <a href="http://www.aionsource.com/forum/crafting-discussion/79040-material-calculator-new-post.html" target="_blank" tooltip="<%= RecipeCalculator.s36 %>">
                                    <%= RecipeCalculator.s37 %></a>
                </td>
            </tr>
        </table>
        <br />
        <br />
        <br />
        <div class="ph phEmpty">
            <%= RecipeCalculator.s40 %>
        </div>
        <div class="ph phDefault dn">
            <table style="width: 100%">
                <tr>
                    <td class="recipePane">
                        <div class="recipes">
                            <h2>
                                <%= RecipeCalculator.s41 %>
                                <a href="javascript:;" class="recipeLink openAionArmory" recipeid="0" style="position: relative; top: -5px" target="_blank" tooltip="<%= RecipeCalculator.s42 %>">
                                    <img src="/Images/arrow-045-small.png" /></a> <a href="javascript:;" class="recipeInfoLink viewInformation" recipeid="0" style="position: relative; top: -5px; left: -5px" target="_blank" tooltip="<%= RecipeCalculator.s43 %>">
                                        <img src="/Images/information-small.png" /></a>
                            </h2>
                            <br />
                            <div class="result">
                            </div>
                        </div>
                    </td>
                    <td class="resultsPane">
                        <div class="total">
                        </div>
                    </td>
                </tr>
            </table>
            <br />
        </div>
        <%--<div style="width: 100%; text-align: center; padding-top: 20px">
			<script type="text/javascript"><!--
				google_ad_client = "pub-0624859065898875";
				/* RecipeCalculator 468x60 */
				google_ad_slot = "8096597491";
				google_ad_width = 468;
				google_ad_height = 60;
			//-->
			</script>
			<script type="text/javascript" src="http://pagead2.googlesyndication.com/pagead/show_ads.js"></script>
		</div>--%>
        <div class="comment" style="padding-top: 30px">
            <%= RecipeCalculator.s44 %>
            <a href="javascript:;" class="feedback comment">
                <%= RecipeCalculator.s45 %></a>
        </div>
    </div>
    <script type="text/javascript">
        var ItemIconBaseDir = '<%= ConfigurationManager.AppSettings["ItemIconBaseDir"] %>';
        var Lang = '<%= this.currentLanguage.Name %>';
        var Version = '<%= this.version %>';
    </script>
    <script type="text/javascript" src="http://<%= this.currentLanguage.Name == "en" || this.currentLanguage.Name == "ru" ? "www" : this.currentLanguage.Name %>.aionarmory.com/js/extooltips.js"></script>
    <script type="text/javascript" src="/LocalizationJs.axd?l=<%= this.currentLanguage.Name %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.Config.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.Parameters.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.Recipe.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.Item.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.SelectItemDialog.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.Utils.js?v=<%= this.version %>"></script>
    <script type="text/javascript" src="/Scripts/RecipeCalculator.js?v=<%= this.version %>"></script>
</asp:Content>
