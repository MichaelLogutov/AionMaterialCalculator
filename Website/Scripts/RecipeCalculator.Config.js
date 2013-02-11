var Config =
{
	//priceCoefficient: 1.0,
	feedbackEmail: null,
	itemPrices: {},
	recipeList:
	{
		race: "",
		skill: "",
		filter: "",
		sortBy: "",
		sortDirection: "",
		pageIndex: 0,
		quality: null,
		types: null,
		itemLevelMin: null,
		itemLevelMax: null,
		skillLevelMin: null,
		skillLevelMax: null,
		ignoreCase: null,
		procVersions: null
	},
	Load: function()
	{
		// item prices
		{
			var c_parts = ($.cookie("Aion.RC.iP") || "").split(",");
			for (var k = 0; k < c_parts.length; k++)
			{
				var kv = c_parts[k].split(":");
				if (kv.length != 2)
					continue;

				this.itemPrices[kv[0]] = parseInt(kv[1], 10);
			}
		}

		// recipe list
		{
			var kv = ($.cookie("Aion.RC.rL") || "").split(":");
			if (kv)
			{
				if (kv.length > 0)
					this.recipeList.race = kv[0];

				if (kv.length > 1)
					this.recipeList.skill = kv[1];

				if (kv.length > 2)
					this.recipeList.sortBy = kv[2];

				if (kv.length > 3)
					this.recipeList.sortDirection = kv[3];

				if (kv.length > 4)
					this.recipeList.pageIndex = parseInt(kv[4], 10);

				if (kv.length > 5)
					this.recipeList.itemLevelMin = parseInt(kv[5], 10);

				if (kv.length > 6)
					this.recipeList.itemLevelMax = parseInt(kv[6], 10);

				if (kv.length > 7)
					this.recipeList.skillLevelMin = parseInt(kv[7], 10);

				if (kv.length > 8)
					this.recipeList.skillLevelMax = parseInt(kv[8], 10);

				if (kv.length > 9)
					this.recipeList.ignoreCase = (parseInt(kv[9], 10) == 1);
				else
					this.recipeList.ignoreCase = true;
			}

			if (!this.recipeList.race)
				this.recipeList.race = "0";

			if (!this.recipeList.skill)
				this.recipeList.skill = "0";

			if (!this.recipeList.sortBy)
				this.recipeList.sortBy = "sl";

			if (!this.recipeList.sortDirection)
				this.recipeList.sortDirection = "asc";

			if (!this.recipeList.pageIndex || isNaN(this.recipeList.pageIndex))
				this.recipeList.pageIndex = 0;

			if (!this.recipeList.itemLevelMin || isNaN(this.recipeList.itemLevelMin))
				this.recipeList.itemLevelMin = null;

			if (!this.recipeList.itemLevelMax || isNaN(this.recipeList.itemLevelMax))
				this.recipeList.itemLevelMax = null;

			if (!this.recipeList.skillLevelMin || isNaN(this.recipeList.skillLevelMin))
				this.recipeList.skillLevelMin = null;

			if (!this.recipeList.skillLevelMax || isNaN(this.recipeList.skillLevelMax))
				this.recipeList.skillLevelMax = null;

			this.recipeList.filter = $.cookie("Aion.RC.rLF") || "";

			// quality
			{
				this.recipeList.quality = [];

				var parts = ($.cookie("Aion.RC.rLQ") || "").split(",");
				for (var k = 0; k < parts.length; k++)
				{
					var n = parseInt(parts[k], 10);
					if (!isNaN(n))
						this.recipeList.quality.push(n);
				}
			}

			// types
			{
				this.recipeList.types = [];

				var parts = ($.cookie("Aion.RC.rLT") || "").split(",");
				for (var k = 0; k < parts.length; k++)
				{
					var n = parseInt(parts[k], 10);
					if (!isNaN(n))
						this.recipeList.types.push(n);
				}
			}

			// proc versions
			{
				this.recipeList.procVersions = [];

				var parts = ($.cookie("Aion.RC.rLPv") || "").split(",");
				for (var k = 0; k < parts.length; k++)
				{
					var n = parseInt(parts[k], 10);
					if (!isNaN(n))
						this.recipeList.procVersions.push(n);
				}
			}
		}

		this.feedbackEmail = $.cookie("feedbackEmail");
	},
	Save: function()
	{
		var cookie_options = { expires: new Date(2100, 1, 1) };

		// item prices
		{
			var c = "";
			$.each(this.itemPrices, function(k, v)
			{
				if (v > 0)
				{
					if (c.length > 0)
						c += ",";

					c += k + ":" + v;
				}
			});

			$.cookie("Aion.RC.iP", c, cookie_options);
		}

		// recipe list
		$.cookie("Aion.RC.rL",
			this.recipeList.race + ":" +
			this.recipeList.skill + ":" +
			this.recipeList.sortBy + ":" +
			this.recipeList.sortDirection + ":" +
			this.recipeList.pageIndex + ":" +
			(this.recipeList.itemLevelMin || "") + ":" +
			(this.recipeList.itemLevelMax || "") + ":" +
			(this.recipeList.skillLevelMin || "") + ":" +
			(this.recipeList.skillLevelMax || "") + ":" +
			(this.recipeList.ignoreCase ? "1" : "0"),
			cookie_options
		);
		$.cookie("Aion.RC.rLF", this.recipeList.filter, cookie_options);
		$.cookie("Aion.RC.rLQ", this.recipeList.quality.join(","));
		$.cookie("Aion.RC.rLT", this.recipeList.types.join(","));
		$.cookie("Aion.RC.rLPv", this.recipeList.procVersions.join(","));

		// other
		$.cookie("feedbackEmail", this.feedbackEmail, cookie_options);
	}
};