import api from "./api";

export async function getRoles() {
  const response = await api.get("/roles");
  return response.data;
}

export async function seedRoles() {
  const response = await api.post("/roles/seed-default");
  return response.data;
}