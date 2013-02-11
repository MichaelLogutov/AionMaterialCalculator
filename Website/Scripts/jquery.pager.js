//
// Author: Logutov Michael
// Date: 24 november 2008
// Description: This plugin will display paging link buttons
// Options:
//		pageSize - amount of elements on the page (default is 10)
//		displayPages - amount of page numbers to display (default is 5)
//		items - array of items (for example id) - will be passed to "changed" callback (default is empty array)
//		changed(items, pageNumber, startIndex, endIndex) - function that will be fired when current page changed
//		autofire - if true then will fire "changed" callback after initialization (default is true)
//		currentPage - initial value for currently selected page index
//

; (function($)
{

	$.fn.extend({
		pager: function(options)
		{
			return this.each(function()
			{
				var t = $(this);
				var opt = $.extend({
					pageSize: 10,
					displayPages: 5,
					items: new Array(),
					changed: function(items, pageNumber, startIndex, endIndex, pager) { },
					autofire: true,
					currentPage: 0
				}, options);

				opt.totalPages = Math.ceil(opt.items.length / opt.pageSize);

				if (!opt.currentPage || opt.currentPage < 0 || isNaN(opt.currentPage))
					opt.currentPage = 0;
				else if (opt.currentPage >= opt.totalPages)
					opt.currentPage = opt.totalPages - 1;

				t.data("pagerOptions", opt);
				t.pagerRegenerate();

				if (opt.autofire)
					t.pagerSetPage(opt.currentPage, false);
			});
		},

		pagerRegenerate: function()
		{
			return this.each(function()
			{
				var t = $(this);
				var opt = t.data("pagerOptions");
				var html = new Array();

				t.empty();

				if (opt.totalPages < 2)
					return;

				var first_index = Math.floor(opt.currentPage / opt.displayPages) * opt.displayPages;
				var last_index = Math.min(first_index + opt.displayPages, opt.totalPages);

				if (opt.currentPage != 0)
					html.push("<a href='javascript:;' page='0' class='pagerPage pagerFirstPage'>1</a><span class='pagerDisabled'> … </span>");
				else
					html.push("<a href='javascript:;' class='pagerPage pagerFirstPage pagerDisabled'>1</a><span class='pagerDisabled'> … </span>");

				if (opt.currentPage > 0)
					html.push("<a href='javascript:;' page='" + (opt.currentPage - 1) + "' class='pagerBack'>&laquo;&laquo;&laquo;</a> ");
				else
					html.push("<a href='javascript:;' class='pagerBack pagerDisabled'>&laquo;&laquo;&laquo;</a> ");

				for (k = first_index; k < last_index; k++)
					html.push("<a href='javascript:;' page='" + k + "' class='pagerPage" + (k == opt.currentPage ? " pagerCurrentPage" : "") + "'>" + (k + 1) + "</a> ");

				if (opt.currentPage + 1 < opt.totalPages)
					html.push("<a href='javascript:;' page='" + (opt.currentPage + 1) + "' class='pagerForward'>&raquo;&raquo;&raquo;</a>");
				else
					html.push("<a href='javascript:;' class='pagerForward pagerDisabled'>&raquo;&raquo;&raquo;</a> ");

				if (opt.currentPage != opt.totalPages - 1)
					html.push("<span class='pagerDisabled'> … </span><a href='javascript:;' page='" + (opt.totalPages - 1) + "' class='pagerPage pagerLastPage'>" + opt.totalPages + "</a>");
				else
					html.push("<span class='pagerDisabled'> … </span><a href='javascript:;' class='pagerPage pagerLastPage pagerDisabled'>" + opt.totalPages + "</a>");

				t.html(html.join(""));

				$("a[page]", t).click(function()
				{
					t.pagerSetPage(parseInt($(this).attr("page")), false);
					return false;
				});
			});
		},

		pagerSetPage: function(index, silence)
		{
			var index = Number(index);

			if (!index || index < 0 || isNaN(index))
				index = 0;

			return this.each(function()
			{
				var t = $(this);
				var opt = t.data("pagerOptions");
				opt.currentPage = index;

				var start_index = index * opt.pageSize;
				var end_index = Math.min((index + 1) * opt.pageSize - 1, opt.items.length - 1);

				t.pagerRegenerate();

				if (!silence)
					opt.changed(opt.items, index, start_index, end_index, t);
			});
		},

		pagerGetPage: function()
		{
			var opt = $(this).data("pagerOptions");
			if (!opt)
				return null;

			return opt.currentPage;
		}
	});

})(jQuery);