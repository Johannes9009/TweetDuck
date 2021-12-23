enabled(){
  const clearColumn = (columnName) => {
    TD.controller.columnManager.get(columnName).clear();
    TD.controller.stats.columnActionClick("clear");
  };
  
  const resetColumn = (columnName) => {
    let col = TD.controller.columnManager.get(columnName);
    col.model.setClearedTimestamp(0);
    col.reloadTweets();
  };
  
  const forEachColumn = (func) => {
    Object.keys(TD.controller.columnManager.getAll()).forEach(func);
  };
  
  let wasShiftPressed = false;
  
  const updateShiftState = (pressed) => {
    if (pressed != wasShiftPressed){
      wasShiftPressed = pressed;
      
      if (pressed){
        $(document).on("mousemove", this.eventKeyUp);
      }
      else{
        $(document).off("mousemove", this.eventKeyUp);
      }
      
      $(".clear-columns-btn-all").text(pressed ? "Restore columns" : "Clear columns");
    }
  };
  
  // event handlers
  
  this.eventClickOneCapture = function(e){
    if (e.target.getAttribute("data-action") === "td-clearcolumns-dosingle"){
      let name = $(e.target).closest(".js-column").attr("data-column");
      e.shiftKey ? resetColumn(name) : clearColumn(name);
      
      e.preventDefault();
      e.stopPropagation();
      e.stopImmediatePropagation();
    }
  };
  
  this.eventClickAll = function(e){
    forEachColumn(e.shiftKey ? resetColumn : clearColumn);
  };
  
  this.eventKeyDown = function(e){
    if (!(document.activeElement === null || document.activeElement === document.body)) {
      return;
    }
    
    updateShiftState(e.shiftKey);
    
    if (e.keyCode === 46){ // 46 = delete
      if (e.altKey){
        forEachColumn(e.shiftKey ? resetColumn : clearColumn);
      }
      else{
        let focusedColumn = $(".js-column.is-focused");
        
        if (focusedColumn.length){
          let name = focusedColumn.attr("data-column");
          e.shiftKey ? resetColumn(name) : clearColumn(name);
        }
      }
    }
  };
  
  this.eventKeyUp = function(e){
    if (!e.shiftKey){
      updateShiftState(false);
    }
  };
  
  this.eventKeyboardShortcuts = function(e){
    $(".keyboard-shortcut-list").first().append(`
<dd class="keyboard-shortcut-definition" style="white-space:nowrap">
  <span class="text-like-keyboard-key">1</span> … <span class="text-like-keyboard-key">9</span> + <span class="text-like-keyboard-key">Del</span> Clear column 1－9
</dd>
<dd class="keyboard-shortcut-definition">
  <span class="text-like-keyboard-key">Alt</span> + <span class="text-like-keyboard-key">Del</span> Clear all columns
</dd>`);
  };
  
  // update UI
  
  this.btnClearAllHTML = `
<a class="clear-columns-btn-all-parent js-header-action link-clean cf app-nav-link padding-h--16 padding-v--2" data-title="Clear columns (hold Shift to restore)" data-action="td-clearcolumns-doall">
  <div class="obj-left margin-l--2"><i class="icon icon-medium icon-clear-timeline"></i></div>
  <div class="clear-columns-btn-all nbfc padding-ts hide-condensed txt-size--14 app-nav-link-text">Clear columns</div>
</a>`;
  
  this.btnClearOneHTML = `
<a class="js-action-header-button column-header-link" href="#" data-action="td-clearcolumns-dosingle">
  <i class="icon icon-clear-timeline js-show-tip" data-placement="bottom" data-original-title="Clear column (hold Shift to restore)" data-action="td-clearcolumns-dosingle"></i>
</a>`;
  
  this.prevNavMenuMustache = TD.mustaches["menus/column_nav_menu.mustache"];
  window.TDPF_injectMustache("menus/column_nav_menu.mustache", "replace", "{{_i}}Add column{{/i}}</div> </a> </div>", `{{_i}}Add column{{/i}}</div></a>${this.btnClearAllHTML}</div>`);
  
  this.prevColumnHeaderMustache = TD.mustaches["column/column_header.mustache"];
  window.TDPF_injectMustache("column/column_header.mustache", "prepend", "<a data-testid=\"optionsToggle\"", this.btnClearOneHTML);
  
  if (TD.ready){
    $(".js-header-add-column").after(this.btnClearAllHTML);
    $("a[data-testid='optionsToggle']", ".js-column-header").before(this.btnClearOneHTML);
  }
  
  // styles
  
  if (!document.getElementById("td-clearcolumns-workaround")){
    // TD started caching mustaches so disabling the plugin doesn't update the column headers properly...
    let workaround = document.createElement("style");
    workaround.id = "td-clearcolumns-workaround";
    workaround.innerText = "#tduck a[data-action='td-clearcolumns-dosingle'] { display: none }";
    document.head.appendChild(workaround);
  }
  
  this.css = window.TDPF_createCustomStyle(this);
  
  this.css.insert(".js-app-add-column.is-hidden + .clear-columns-btn-all-parent { display: none; }");
  this.css.insert(".column-navigator-overflow .clear-columns-btn-all-parent { display: none !important; }");
  this.css.insert(".column-navigator-overflow { bottom: 224px !important; }");
  this.css.insert(".app-navigator .clear-columns-btn-all-parent { font-weight: 700; }");
  
  this.css.insert(".column-header-links { min-width: 51px !important; }");
  this.css.insert(".column[data-td-icon='icon-message'] .column-header-links { min-width: 110px !important; }");
  this.css.insert(".btn-options-tray[data-action='clear'] { display: none !important; }");
  
  this.css.insert("#tduck a[data-action='td-clearcolumns-dosingle'] { display: inline-block; }");
  this.css.insert("#tduck .column[data-td-icon='icon-schedule'] a[data-action='td-clearcolumns-dosingle'] { display: none; }");
  this.css.insert("#tduck .column[data-td-icon='icon-custom-timeline'] a[data-action='td-clearcolumns-dosingle'] { display: none; }");
}

ready(){
  document.addEventListener("click", this.eventClickOneCapture, true);
  $(document).on("click", "[data-action='td-clearcolumns-doall']", this.eventClickAll);
  $(document).on("keydown", this.eventKeyDown);
  $(document).on("keyup", this.eventKeyUp);
  $(document).on("uiShowKeyboardShortcutList", this.eventKeyboardShortcuts);
  
  $(".js-app-add-column").first().after(this.btnClearAllHTML);
  
  // fix tooltip
  
  let tooltipEvents = $._data($(".js-header-action")[0], "events");
  
  if (tooltipEvents.mouseover && tooltipEvents.mouseover.length && tooltipEvents.mouseout && tooltipEvents.mouseout.length){
    $(".clear-columns-btn-all-parent").on({
      mouseover: tooltipEvents.mouseover[0].handler,
      mouseout: tooltipEvents.mouseout[0].handler
    });
  }
}

disabled(){
  this.css.remove();
  
  document.removeEventListener("click", this.eventClickOneCapture);
  $(document).off("click", "[data-action='td-clearcolumns-doall']", this.eventClickAll);
  $(document).off("keydown", this.eventKeyDown);
  $(document).off("keyup", this.eventKeyUp);
  $(document).off("uiShowKeyboardShortcutList", this.eventKeyboardShortcuts);
  
  TD.mustaches["menus/column_nav_menu.mustache"] = this.prevNavMenuMustache;
  TD.mustaches["column/column_header.mustache"] = this.prevColumnHeaderMustache;
  
  $("[data-action^='td-clearcolumns-']").remove();
}
