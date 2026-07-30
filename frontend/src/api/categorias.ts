import type { CategoriaDto } from '../types/categorias'
import { httpRequest } from './http'

export function listarCategorias(
  signal?: AbortSignal,
): Promise<CategoriaDto[]> {
  return httpRequest<CategoriaDto[]>('/api/v1/categorias', { signal })
}
