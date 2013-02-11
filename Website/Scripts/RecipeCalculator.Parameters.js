var Parameters =
{
	recipeId: NaN,
	itemId: NaN,
	haveCount: {},
	itemPrices: {},
	itemProbabilities: {},
	checkedItems: [],
	totalQuantity: NaN,
	sellPrice: NaN,
	Reset: function()
	{
		this.recipeId = NaN;
		this.itemId = NaN;
		this.haveCount = {};
		this.itemPrices = {};
		this.itemProbabilities = {};
		this.checkedItems = [];
		this.totalQuantity = NaN;
		this.sellPrice = NaN;
	},
	Load: function()
	{
		this.recipeId = parseInt($.queryString.hash["recipeId"], 10);
		this.itemId = parseInt($.queryString.hash["itemId"], 10);
		this.totalQuantity = parseInt($.queryString.hash["tQ"], 10);
		this.sellPrice = parseInt($.queryString.hash["sP"], 10);

		// items info
		this.haveCount = {};
		this.itemPrices = {};
		this.itemProbabilities = {};

		var items_info = ($.queryString.hash["i"] || "").split(",");
		for (var k = 0; k < items_info.length; k++)
		{
			var item_info = items_info[k].split(":");
			if (item_info.length < 1)
				continue;

			var id = parseInt(item_info[0], 10);
			if (isNaN(id))
				continue;

			// have
			if (item_info.length > 1)
			{
				var n = parseInt(item_info[1], 10);
				if (!isNaN(n) && n > 0)
					this.haveCount[id] = n;
			}

			// price
			if (item_info.length > 2)
			{
				var n = parseInt(item_info[2], 10);
				if (!isNaN(n) && n > 0)
					this.itemPrices[id] = n;
			}

			// probability
			if (item_info.length > 3)
			{
				var n = parseFloat(item_info[3]);
				if (!isNaN(n) && n > 1 && n < 100)
					this.itemProbabilities[id] = n;
			}
		}

		// checked items
		this.checkedItems = [];
		var ci = ($.queryString.hash["cI"] || "").split(",");
		for (var k = 0; k < ci.length; k++)
		{
			var id = parseInt(ci[k], 10);
			if (!isNaN(id))
				this.checkedItems.push(id);
		}
	},
	Save: function()
	{
		$.queryString.hash["recipeId"] = isNaN(this.recipeId) ? null : this.recipeId;
		$.queryString.hash["itemId"] = isNaN(this.itemId) ? null : this.itemId;
		$.queryString.hash["tQ"] = isNaN(this.totalQuantity) ? null : this.totalQuantity;
		$.queryString.hash["sP"] = isNaN(this.sellPrice) ? null : this.sellPrice;

		var items_info = {};

		var _SetItemInfo = function(id, index, n)
		{
			var item_info = items_info[id];
			if (!item_info)
			{
				item_info = ["", "", ""];
				items_info[id] = item_info;
			}

			item_info[index] = n;
		};

		// have count
		{
			$.each(this.haveCount, function(k, v)
			{
				if (!isNaN(v) && v > 0)
					_SetItemInfo(k, 0, v);
			});
		}

		// item prices
		{
			$.each(this.itemPrices, function(k, v)
			{
				if (!isNaN(v) && v > 0)
					_SetItemInfo(k, 1, v);
			});
		}

		// item probabilities
		{
			$.each(this.itemProbabilities, function(k, v)
			{
				if (!isNaN(v) && v > 1 && v < 100)
					_SetItemInfo(k, 2, v);
			});
		}

		var i = "";
		$.each(items_info, function(k, v)
		{
			if (i.length > 0)
				i += ",";

			i += k + ":" + v.join(":");
		});

		if (i.length < 1)
			i = null;

		$.queryString.hash["i"] = i;

		// checked items
		if (this.checkedItems.length > 0)
			$.queryString.hash["cI"] = this.checkedItems.join(",");
		else
			$.queryString.hash["cI"] = null;

		$.queryString.save();
	},
	CheckItemsChanged: function(recipe)
	{
		this.checkedItems = [];

		for (var k = 0; k < recipe.items.length; k++)
		{
			var item = recipe.items[k];
			if (!item.checked)
				continue;

			this.checkedItems.push(k);
		}

		this.Save();
	}
};