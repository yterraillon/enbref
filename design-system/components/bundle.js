/* @ds-bundle: {"format":4,"namespace":"EnBref","components":[{"name":"Button"},{"name":"IconButton"},{"name":"Switch"},{"name":"SegmentedControl"},{"name":"CheckCircle"},{"name":"ChoiceChips"},{"name":"ListSection"},{"name":"ListRow"},{"name":"LargeTitle"},{"name":"ProgressSegments"},{"name":"SummaryCard"},{"name":"RubricCard"},{"name":"MiniPlayer"},{"name":"TextField"},{"name":"StatusBadge"},{"name":"Icon"}]} */
(function () {
  "use strict";
  var React = window.React;
  var h = React.createElement;
  var useState = React.useState;

  function cx() {
    var out = [];
    for (var i = 0; i < arguments.length; i++) if (arguments[i]) out.push(arguments[i]);
    return out.join(" ");
  }
  function omit(obj, keys) {
    var r = {};
    for (var k in obj) if (Object.prototype.hasOwnProperty.call(obj, k) && keys.indexOf(k) < 0) r[k] = obj[k];
    return r;
  }
  // Controlled when `value` is given, otherwise keeps its own state.
  function useControlled(value, initial, onChange) {
    var st = useState(initial);
    var controlled = value !== undefined;
    var current = controlled ? value : st[0];
    function set(v) {
      if (!controlled) st[1](v);
      if (onChange) onChange(v);
    }
    return [current, set];
  }

  // ---- Icons (paths from the 1b mock-ups; ink = currentColor) ----
  var ICONS = {
    play: { w: 12, h: 14, vb: "0 0 11 13", d: [["path", { d: "M1 1l9 5.5L1 12z", fill: "currentColor" }]] },
    pause: { w: 12, h: 14, vb: "0 0 11 13", d: [["rect", { x: 1, y: 1, width: 3, height: 11, fill: "currentColor" }], ["rect", { x: 7, y: 1, width: 3, height: 11, fill: "currentColor" }]] },
    "skip-forward": { w: 18, h: 14, vb: "0 0 18 14", d: [["path", { d: "M1 1l7 6-7 6zM9 1l7 6-7 6z", fill: "currentColor" }]] },
    check: { w: 11, h: 9, vb: "0 0 11 9", d: [["path", { d: "M1 4.5l3 3L10 1", stroke: "currentColor", strokeWidth: 1.8, fill: "none", strokeLinecap: "round", strokeLinejoin: "round" }]] },
    "chevron-right": { w: 7, h: 12, vb: "0 0 7 12", d: [["path", { d: "M1 1l5 5-5 5", stroke: "currentColor", strokeWidth: 2, fill: "none", strokeLinecap: "round" }]] },
    "chevron-up": { w: 13, h: 8, vb: "0 0 12 8", d: [["path", { d: "M1 7l5-5 5 5", stroke: "currentColor", strokeWidth: 1.8, fill: "none", strokeLinecap: "round" }]] },
    "chevron-down": { w: 13, h: 8, vb: "0 0 12 8", d: [["path", { d: "M1 1l5 5 5-5", stroke: "currentColor", strokeWidth: 1.8, fill: "none", strokeLinecap: "round" }]] }
  };
  function Icon(props) {
    var def = ICONS[props.name];
    if (!def) return null;
    var s = props.size ? props.size / def.h : 1;
    return h("svg", { width: Math.round(def.w * s), height: Math.round(def.h * s), viewBox: def.vb, "aria-hidden": true, focusable: "false", style: { display: "block" } },
      def.d.map(function (p, i) { return h(p[0], Object.assign({ key: i }, p[1])); }));
  }
  function renderIcon(icon) {
    if (!icon) return null;
    return typeof icon === "string" ? h(Icon, { name: icon }) : icon;
  }

  // ---- Button ----
  function Button(props) {
    var variant = props.variant || "primary";
    var rest = omit(props, ["variant", "icon", "block", "className", "children"]);
    return h("button", Object.assign({ type: "button" }, rest, {
      className: cx("eb-btn", "eb-btn--" + variant, props.block && "eb-btn--block", variant === "plain" ? "subhead" : "headline", props.className)
    }), renderIcon(props.icon), props.children != null ? h("span", null, props.children) : null);
  }

  // ---- IconButton ----
  function IconButton(props) {
    var variant = props.variant || "surface";
    var rest = omit(props, ["variant", "icon", "label", "className", "children"]);
    return h("button", Object.assign({ type: "button", "aria-label": props.label, title: props.label }, rest, {
      className: cx("eb-icon-btn", "eb-icon-btn--" + variant, "headline", props.className)
    }), props.children != null ? props.children : renderIcon(props.icon));
  }

  // ---- Switch ----
  function Switch(props) {
    var c = useControlled(props.checked, !!props.defaultChecked, props.onChange);
    return h("button", {
      type: "button", role: "switch", "aria-checked": c[0] ? "true" : "false", "aria-label": props.label,
      disabled: props.disabled, className: cx("eb-switch", props.className),
      onClick: function () { c[1](!c[0]); }
    });
  }

  // ---- SegmentedControl ----
  function SegmentedControl(props) {
    var opts = props.options || [];
    var c = useControlled(props.value, props.defaultValue !== undefined ? props.defaultValue : opts[0], props.onChange);
    return h("div", { role: "radiogroup", "aria-label": props.label, className: cx("eb-seg", props.className) },
      opts.map(function (o) {
        return h("button", { key: o, type: "button", role: "radio", "aria-checked": o === c[0] ? "true" : "false", className: "eb-seg-opt control", onClick: function () { c[1](o); } }, o);
      }));
  }

  // ---- CheckCircle ----
  function CheckCircle(props) {
    var c = useControlled(props.checked, !!props.defaultChecked, props.onChange);
    return h("button", { type: "button", role: "checkbox", "aria-checked": c[0] ? "true" : "false", className: cx("eb-check", "body", props.className), onClick: function () { c[1](!c[0]); } },
      h("span", { className: "eb-check-dot" }, c[0] ? h(Icon, { name: "check" }) : null),
      props.children != null ? h("span", null, props.children) : null);
  }

  // ---- ChoiceChips ----
  function ChoiceChips(props) {
    var opts = props.options || [];
    var c = useControlled(props.value, props.defaultValue, props.onChange);
    return h("div", { role: "radiogroup", "aria-label": props.label, className: cx("eb-chips", props.className), style: { gridTemplateColumns: "repeat(" + (props.columns || 3) + ", minmax(0, 1fr))" } },
      opts.map(function (o) {
        return h("button", { key: o, type: "button", role: "radio", "aria-checked": o === c[0] ? "true" : "false", className: "eb-chip callout", onClick: function () { c[1](o); } }, o);
      }));
  }

  // ---- ListSection ----
  function ListSection(props) {
    return h("section", { className: cx("eb-section", props.className) },
      props.header ? h("div", { className: "eb-section-head footnote" }, props.header) : null,
      h("div", { className: "eb-card" }, props.children),
      props.footer ? h("div", { className: "eb-section-foot footnote" }, props.footer) : null);
  }

  // ---- ListRow ----
  function ListRow(props) {
    var tall = !!props.subtitle;
    var trailing = [];
    if (props.value != null) trailing.push(h("span", { key: "v", className: "eb-row-value body" }, props.value));
    if (props.action != null) trailing.push(h("span", { key: "a", className: "eb-row-action subhead", style: { fontWeight: 600 } }, props.action));
    if (props.onMoveUp || props.onMoveDown) trailing.push(h("span", { key: "r", className: "eb-reorder-group" },
      h("button", { type: "button", className: "eb-reorder", "aria-label": "Monter", disabled: !props.onMoveUp, onClick: props.onMoveUp }, h(Icon, { name: "chevron-up" })),
      h("button", { type: "button", className: "eb-reorder", "aria-label": "Descendre", disabled: !props.onMoveDown, onClick: props.onMoveDown }, h(Icon, { name: "chevron-down" }))));
    if (props.trailing) trailing.push(h(React.Fragment, { key: "t" }, props.trailing));
    if (props.chevron) trailing.push(h("span", { key: "c", className: "eb-row-chev" }, h(Icon, { name: "chevron-right" })));
    var cls = cx("eb-row", props.compact && "eb-row--compact", tall && "eb-row--tall", props.dimmed && "eb-row--dimmed", props.onClick && "eb-row-btn", props.className);
    var main = h("div", { className: "eb-row-main" },
      h("div", { className: "body" }, props.title),
      tall ? h("div", { className: "eb-row-sub footnote" }, props.subtitle) : null);
    return props.onClick
      ? h("button", { type: "button", className: cls, onClick: props.onClick }, main, trailing)
      : h("div", { className: cls }, main, trailing);
  }

  // ---- LargeTitle ----
  function LargeTitle(props) {
    return h("header", { className: cx("eb-bar", props.className) },
      h("div", null,
        props.eyebrow ? h("div", { className: "eb-bar-eyebrow eyebrow" }, props.eyebrow) : null,
        h("h1", { className: "eb-bar-title large-title", style: { margin: 0 } }, props.title)),
      props.trailing || null);
  }

  // ---- ProgressSegments ----
  function ProgressSegments(props) {
    var segs = props.segments || [];
    return h("div", { className: cx("eb-progress", props.className), role: "img", "aria-label": props.label },
      segs.map(function (s, i) { return h("div", { key: i, className: cx("eb-progress-seg", s !== "todo" && "eb-progress-seg--" + s) }); }));
  }

  // ---- SummaryCard ----
  function SummaryCard(props) {
    var read = props.read || 0, total = props.total || 0;
    return h("div", { className: cx("eb-summary", props.className) },
      h("div", { className: "eb-summary-top" },
        h("span", { className: "headline" }, read + " sur " + total + " rubriques lues"),
        props.minutes != null ? h("span", { className: "subhead eb-muted" }, props.minutes + " min") : null),
      h(ProgressSegments, { segments: props.segments, label: read + " sur " + total + " rubriques lues" }),
      h(Button, { variant: "primary", block: true, icon: props.playing ? "pause" : "play", onClick: props.onPlay }, props.playLabel || (props.playing ? "Pause" : "Écouter le récap")));
  }

  // ---- RubricCard ----
  function RubricCard(props) {
    var size = props.textSize || "m";
    var c = useControlled(props.read, !!props.defaultRead, props.onReadChange);
    if (c[0]) {
      return h("div", { className: cx("eb-rubric", props.className) },
        h("button", { type: "button", className: "eb-collapsed", onClick: function () { c[1](false); }, "aria-label": props.name + ", lue. Relire" },
          h("span", { className: "eb-check-dot" }, h(Icon, { name: "check" })),
          h("span", { className: "eb-collapsed-name headline" }, props.name),
          h("span", { className: "eb-collapsed-again subhead" }, "Relire")));
    }
    return h("div", { className: cx("eb-rubric", props.className) },
      h("div", { className: "eb-rubric-head" },
        h("span", { className: "eb-rubric-name eyebrow" }, props.name),
        props.current ? h("span", { className: "eb-rubric-now eyebrow" }, "● En lecture") : null),
      (props.items || []).map(function (it, i) {
        return h("article", { key: i, className: "eb-brief" },
          h("div", { className: "eb-brief-title article-title-" + size }, it.title),
          h("div", { className: "eb-brief-body article-body-" + size }, it.body));
      }),
      h(CheckCircle, { checked: false, onChange: function () { c[1](true); } }, "Marquer comme lu"));
  }

  // ---- MiniPlayer ----
  function MiniPlayer(props) {
    return h("div", { className: cx("eb-player", props.className), role: "region", "aria-label": "Lecture en cours" },
      h("div", { className: "eb-player-text" },
        h("div", { className: "eb-player-label caption" }, props.label || "EnBref · en lecture"),
        h("div", { className: "eb-player-title callout" }, props.title)),
      h(IconButton, { variant: "ghost", label: "Vitesse de lecture", onClick: props.onRate }, h("span", { className: "subhead", style: { fontWeight: 600 } }, props.rate || "1×")),
      h(IconButton, { variant: "ghost", icon: "skip-forward", label: "Rubrique suivante", onClick: props.onNext }),
      h(IconButton, { variant: "filled", icon: props.playing === false ? "play" : "pause", label: props.playing === false ? "Lecture" : "Pause", onClick: props.onToggle }));
  }

  // ---- TextField (admin) ----
  function TextField(props) {
    var id = props.id || ("eb-f-" + String(props.label || "").replace(/\W+/g, "-").toLowerCase());
    var rest = omit(props, ["label", "hint", "error", "className", "id"]);
    return h("div", { className: cx("eb-field", props.error && "eb-field--error", props.className) },
      props.label ? h("label", { htmlFor: id, className: "eb-field-label footnote" }, props.label) : null,
      h("input", Object.assign({ id: id, className: "body", "aria-invalid": props.error ? "true" : undefined }, rest)),
      (props.error || props.hint) ? h("div", { className: "eb-field-hint footnote" }, props.error || props.hint) : null);
  }

  // ---- StatusBadge (admin) ----
  function StatusBadge(props) {
    return h("span", { className: cx("eb-badge", "eb-badge--" + (props.tone || "neutral"), "footnote", props.className), style: { fontWeight: 600 } }, props.children);
  }

  var api = {
    Button: Button, IconButton: IconButton, Switch: Switch, SegmentedControl: SegmentedControl,
    CheckCircle: CheckCircle, ChoiceChips: ChoiceChips, ListSection: ListSection, ListRow: ListRow,
    LargeTitle: LargeTitle, ProgressSegments: ProgressSegments, SummaryCard: SummaryCard, RubricCard: RubricCard,
    MiniPlayer: MiniPlayer, TextField: TextField, StatusBadge: StatusBadge, Icon: Icon
  };
  window.EnBref = Object.assign(window.EnBref || {}, api);
})();
