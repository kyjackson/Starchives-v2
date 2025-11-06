function loadScript(sourceUrl) {
	if (sourceUrl.Length == 0) {
		console.error("Invalid source URL");
		return;
	}

	var tag = document.createElement('script');
	tag.src = sourceUrl;
	tag.type = "text/javascript";

	tag.onload = function () {
		console.log("Script loaded successfully");
	}

	tag.onerror = function () {
		console.error("Failed to load script");
	}

	document.body.appendChild(tag);
}

// Scroll the element with the provided id into view (used by Console.razor)
function scrollAnchorIntoView(id) {
	try {
		var el = document.getElementById(id);
		if (!el) return;
		// prefer smooth scrolling to bottom of container
		el.scrollIntoView({ behavior: "smooth", block: "end" });
	} catch (e) {
		console.error("scrollAnchorIntoView error:", e);
	}
}
