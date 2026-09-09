using backend.DTOs;
using backend.Repositories.RecommendationRepository;

namespace backend.Services.RecommendationService
{
    public class RecommendationService
    {
        private readonly RecommendationRepository _recommendationRepository;
        /**
         * 构造函数
         * @param recommendationRepository RecommendationRepository 实例
         * @return 无
         */
        public RecommendationService(RecommendationRepository recommendationRepository)
        {
            _recommendationRepository = recommendationRepository;
        }
        /// <summary>
        /// 获取某个读者的推荐书籍
        /// </summary>
        /// <param name="readerId">读者ID</param>
        /// <param name="topN">推荐书籍数量</param>
        /// <returns>推荐书籍列表</returns>
        public async Task<IEnumerable<RecommendedBookDto>> GetRecommendationsAsync(long readerId, int topN = 10)
        {
            return await _recommendationRepository.GetRecommendationAsync(readerId, topN);
        }
    }
}
