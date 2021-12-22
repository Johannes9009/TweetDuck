import { $ } from "../api/jquery.js";
import { TD } from "../api/td.js";
import { ensurePropertyExists } from "../api/utils.js";

const pinHTML = `
<svg id="td-compose-drawer-pin" viewBox="0 0 24 24" class="icon js-show-tip" data-original-title="Stay open" data-tooltip-position="left">
  <path d="M9.884,16.959l3.272,0.001l-0.82,4.568l-1.635,0l-0.817,-4.569Z"/>
  <rect x="8.694" y="7.208" width="5.652" height="7.445"/>
  <path d="M16.877,17.448c0,-1.908 -1.549,-3.456 -3.456,-3.456l-3.802,0c-1.907,0 -3.456,1.548 -3.456,3.456l10.714,0Z"/>
  <path d="M6.572,5.676l2.182,2.183l5.532,0l2.182,-2.183l0,-1.455l-9.896,0l0,1.455Z"/>
</svg>
`;

/**
 * Adds a pin icon to make tweet compose drawer stay open.
 */
export default function() {
	ensurePropertyExists(TD, "settings", "getComposeStayOpen");
	ensurePropertyExists(TD, "settings", "setComposeStayOpen");
	
	$(document).on("tduckOldComposerActive", function() {
		document.querySelector(".js-docked-compose .js-compose-header").insertAdjacentHTML("beforeend", pinHTML);
		
		const pin = document.getElementById("td-compose-drawer-pin");
		
		pin.addEventListener("click", function() {
			if (TD.settings.getComposeStayOpen()) {
				pin.style.transform = "rotate(0deg)";
				TD.settings.setComposeStayOpen(false);
			}
			else {
				pin.style.transform = "rotate(90deg)";
				TD.settings.setComposeStayOpen(true);
			}
		});
		
		if (TD.settings.getComposeStayOpen()) {
			pin.style.transform = "rotate(90deg)";
		}
	});
};
