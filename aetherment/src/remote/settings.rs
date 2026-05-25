use std::{io::Write, path::Path};
use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, PartialEq, Deserialize, Serialize)]
pub struct Settings {
	pub auto_update: bool,
	pub origin: String,
}

impl Default for Settings {
	fn default() -> Self {
		Self {
			auto_update: true,
			origin: String::new(),
		}
	}
}

impl Settings {
	fn path_for(mod_id: &str) -> Result<std::path::PathBuf, std::io::Error> {
		let id_hash = crate::hash_str(blake3::hash(mod_id.as_bytes()));
		let config_dir = dirs::config_dir()
			.ok_or_else(|| std::io::Error::new(std::io::ErrorKind::NotFound, "No config dir"))?;
		Ok(config_dir.join("Aetherment").join("remote").join(id_hash))
	}

	pub fn exists(mod_id: &str) -> bool {
		Self::path_for(mod_id).map(|path| path.exists()).unwrap_or(false)
	}
	
	pub fn open_from(path: &Path) -> Self {
		crate::resource_loader::read_json::<Self>(path).unwrap_or_default()
	}
	
	pub fn open(mod_id: &str) -> Self {
		Self::try_open(mod_id).unwrap_or_default()
	}

	pub fn try_open(mod_id: &str) -> Result<Self, std::io::Error> {
		Ok(Self::open_from(&Self::path_for(mod_id)?))
	}
	
	pub fn save_to(&self, path: &Path) {
		let mut f = std::fs::File::create(path).unwrap();
		f.write_all(crate::json_pretty(&self).unwrap().as_bytes()).unwrap()
	}
	
	pub fn save(&self, mod_id: &str) {
		_ = self.try_save(mod_id);
	}

	pub fn try_save(&self, mod_id: &str) -> Result<(), std::io::Error> {
		let path = Self::path_for(mod_id)?;
		if let Some(parent) = path.parent() {
			std::fs::create_dir_all(parent)?;
		}
		self.save_to(&path);
		Ok(())
	}
}
