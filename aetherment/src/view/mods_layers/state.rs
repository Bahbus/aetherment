#[derive(Default)]
pub struct ModsViewModel {
	pub selected_mod: String,
	pub selected_category_tab: String,
	pub gamma: u8,
	pub new_preset_name: String,
	pub last_was_busy: bool,
}

impl ModsViewModel {
	pub fn new() -> Self {
		Self {
			gamma: 50,
			..Default::default()
		}
	}

	pub fn validate_new_preset_name(&self) -> bool {
		!self.new_preset_name.is_empty()
			&& self.new_preset_name != "Custom"
			&& self.new_preset_name != "Default"
	}

	pub fn ensure_valid_category(&mut self, categories: &[&str]) {
		if !categories.contains(&self.selected_category_tab.as_str()) {
			if let Some(first) = categories.first() {
				self.selected_category_tab = (*first).to_string();
			}
		}
	}
}
