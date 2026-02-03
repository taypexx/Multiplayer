using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Controls;

namespace Multiplayer.Static
{
    internal static class ExpressionDeterminer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">The container to choose an <see cref="Expression"/> from.</param>
        /// <param name="msg">The message which will be mapped.</param>
        /// <returns>An <see cref="Expression"/> that matches the <paramref name="msg"/> best.</returns>
        internal static Expression Determine(ExpressionContainer container, string msg)
        {
            // TODO: actually write this thing holy shit

            if (container == null || container.m_Expressions == null || container.m_Expressions.Count == 0) return null; 
            return container.m_Expressions[Random.Shared.Next(0,container.m_Expressions.Count-1)];
        }
    }
}
