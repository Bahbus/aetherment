use std::path::PathBuf;

pub trait ModsDomainOps {
	fn apply_queue_size(&self) -> usize;
	fn mod_enabled(&self, mod_id: &str, collection_id: &str) -> bool;
	fn set_mod_enabled(&self, mod_id: &str, collection_id: &str, enabled: bool);
	fn import_mods(&self, progress: crate::modman::backend::TaskProgress, paths: Vec<PathBuf>);
	fn finalize_apply(&self, progress: crate::modman::backend::TaskProgress);
	fn redraw_self(&self);
}

#[derive(Default)]
pub struct BackendModsDomain;

impl ModsDomainOps for BackendModsDomain {
	fn apply_queue_size(&self) -> usize {
		crate::backend().apply_queue_size()
	}

	fn mod_enabled(&self, mod_id: &str, collection_id: &str) -> bool {
		crate::backend().get_mod_enabled(mod_id, collection_id)
	}

	fn set_mod_enabled(&self, mod_id: &str, collection_id: &str, enabled: bool) {
		crate::backend().set_mod_enabled(mod_id, collection_id, enabled);
	}

	fn import_mods(&self, progress: crate::modman::backend::TaskProgress, paths: Vec<PathBuf>) {
		crate::backend().install_mods_path(progress, paths);
	}

	fn finalize_apply(&self, progress: crate::modman::backend::TaskProgress) {
		crate::backend().finalize_apply(progress);
	}

	fn redraw_self(&self) {
		crate::backend().redraw_self();
	}
}
