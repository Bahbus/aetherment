pub fn draw_mod_category_tabs(
	ui: &mut egui::Ui,
	selected_category_tab: &mut String,
	categories: &[&str],
) {
	if categories.len() <= 1 {
		return;
	}
	ui.horizontal(|ui| {
		for cat in categories.iter() {
			ui.selectable_value(selected_category_tab, cat.to_string(), *cat);
		}
	});
}
